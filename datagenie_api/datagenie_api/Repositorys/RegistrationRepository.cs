using Azure.Core;
using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using datagenie_api.Model.Dto;
using datagenie_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using static datagenie_api.Repositorys.HelperRepository;
using Microsoft.Extensions.Logging;

namespace datagenie_api.Repositorys
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly DapperContext _context;
        private readonly IHelperRepository _Helper;
        private readonly IEmailService _Email;
        private readonly ILogger<RegistrationRepository> _logger;

        public RegistrationRepository(DapperContext context, IHelperRepository helper, IEmailService email, ILogger<RegistrationRepository> logger)
        {
            _context = context;
            _Helper = helper;
            _Email = email;
            _logger = logger;
        }

        public async Task<dynamic> GetOtpRecordAsync(string email, string otp)
        {
            _logger.LogInformation("Fetching OTP record for Email: {Email}, OTP: {Otp}", email, otp);
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<dynamic>(
                "VerifyOtp",
                new { Email = email, Otp = otp },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task MarkOtpVerifiedAsync(string email, string otp)
        {
            _logger.LogInformation("Marking OTP as verified for Email: {Email}, OTP: {Otp}", email, otp);
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "MarkOtpVerified",
                new { Email = email, Otp = otp },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<dynamic> GetTempRegisterDataAsync(string email)
        {
            _logger.LogInformation("Fetching temp registration data for Email: {Email}", email);
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<dynamic>(
                "GetTempRegisterData",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DeleteTempRegisterDataAsync(string email)
        {
            _logger.LogInformation("Deleting temp registration data for Email: {Email}", email);
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "DeleteTempRegisterData",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<string> VerifyAndRegisterUserAsync(RegisterRequest request)
        {
            _logger.LogInformation("Registering user: {Email}", request.Email);
            using var connection = _context.CreateConnection();

            var result = await connection.ExecuteScalarAsync<string>(
                "USP_RegisterNewUser",
                new
                {
                    request.FirstName,
                    request.LastName,
                    UserName = request.UserName,
                    request.Email,
                    Password = EncryptionService.Encrypt(request.Password),
                    request.CompanyName,
                    request.JobTitle
                },
                commandType: CommandType.StoredProcedure
            );

            _logger.LogInformation("User registered. Email: {Email}, Result: {Result}", request.Email, result);
            return result;
        }

        public async Task<string> RegisterEmail(RegisterRequest request, HttpContext httpContext)
        {
            _logger.LogInformation("RegisterEmail started for Email: {Email}", request.Email);

            using var connection = _context.CreateConnection();

            // Check if client exists by email
            var result = await CheckClientExists(request.Email);

            // If the client exists, return a message
            if (result.Count > 0)
            {
                _logger.LogWarning("Email already exists: {Email}", request.Email);
                return "Email already exists.";
            }

            // Generate OTP
            string otp = _Helper.GenerateSecureOtp();
            _logger.LogInformation("Generated OTP for {Email}: {Otp}", request.Email, otp);

            // Insert OTP record into the database
            await InsertOtpRecordAsync(connection, request.Email,request.UserName, otp);
            _logger.LogInformation("Inserted OTP record for {Email}", request.Email);

            // Insert temp registration data
            await InsertTempRegisterDataAsync(connection, request);
            _logger.LogInformation("Inserted temp register data for {Email}", request.Email);

            // Send OTP email to the user
            await SendOtpEmailAsync(request, otp, httpContext);
            _logger.LogInformation("Sent OTP email to {Email}", request.Email);

            return "OTP sent to your email.";
        }

        public async Task<ClientDetails?> CheckUserExists(string? username, string? password, int version = 1)
        {
            _logger.LogInformation("Checking user existence for Username: {Username}", username);
            using var connection = _context.CreateConnection();

            var parameters = new { UserName = username, Password = password, Version = version };

            var result = await connection.QuerySingleOrDefaultAsync<ClientDetails>(
                "usp_CheckUserExists",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            _logger.LogInformation("User existence check completed for {Username}. Found: {Found}", username, result != null);
            return result;
        }

        public async Task<ValidUser> CheckClientExists(string email, string versionId = "V1")
        {
            _logger.LogInformation("Checking if client exists for Email/Username: {Email}", email);

            using var connection = _context.CreateConnection();

            // Execute the stored procedure and map the result to a ResetPasswordRequest object
            var result = await connection.QuerySingleOrDefaultAsync(
                "CheckClientExists",
                new { Email = email, VersionId = versionId },
                commandType: CommandType.StoredProcedure
            );

            // If no result was found, return a default object with count 0 and null values
            if (result == null)
            {
                return new ValidUser
                {
                    Count = 0,
                    Email = null,
                    Username = null
                };
            }

           

            // Map the result to ResetPasswordRequest object and return it
            return new ValidUser
            {
                Count = result.ClientCount,
                Email = result.Email,
                Username = result.UserName
            };
        }



        public async Task SendLoginOtpEmail(string Email, string FirstName, string otp, HttpContext httpContext)
        {
            _logger.LogInformation("Sending login OTP email to: {Email}", Email);

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var browserName = _Helper.GetBrowserInfo(userAgent);
            string[] strArrayMacIDAndIP = _Helper.GetMacIDAndIPAddress();

            TokenModel objToken = new TokenModel
            {
                EmailID = Email,
                OTP = otp,
                FirstName = FirstName,
                UserEmail = Email,
                IPAddress = ipAddress ?? string.Empty,
                MacAddress = strArrayMacIDAndIP[0],
                BrowserName = browserName.Name,
                BrowserVersion = browserName.Version
            };

            _Email.SendEmailFromTemplate(objToken, CommunicationTemplate.AuthenticationCode);
            _logger.LogInformation("Login OTP email sent successfully to {Email}", Email);
        }

        public async Task<TrustedDeviceResponse> UpdateTrustedDeviceNumberAsync(int clientId, string trustedNumber)
        {
            _logger.LogInformation("Updating trusted device number for ClientId: {ClientId}", clientId);
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<TrustedDeviceResponse>(
                "UpdateTrustedDeviceNumber",
                new { ClientID = clientId, TrustedDeviceNumber = trustedNumber },
                commandType: CommandType.StoredProcedure
            );

            _logger.LogInformation("Updated trusted device number for ClientId: {ClientId}", clientId);
            return result;
        }

        public async Task InsertOtpRecordAsync(IDbConnection connection, string? Email, string? username, string otp)
        {
            var otpEntity = new
            {
                Email = Email,
                Username = username,
                OTP = otp,
                IsVerified = false,
                CreatedAt = DateTime.Now,
                OtpType = "registration",
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };

            await connection.ExecuteAsync(
                "sp_InsertEmailOtpVerification",
                otpEntity,
                commandType: CommandType.StoredProcedure
            );
            _logger.LogInformation("OTP record inserted in DB for Email: {Email}",Email);
        }

        public async Task InsertTempRegisterDataAsync(IDbConnection connection, RegisterRequest request)
        {
            var tempRegister = new
            {
                Email = request.Email,
                JsonData = JsonConvert.SerializeObject(request),
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };

            await connection.ExecuteAsync(
                "sp_InsertTempRegisterData",
                tempRegister,
                commandType: CommandType.StoredProcedure
            );
            _logger.LogInformation("Temp register data inserted for Email: {Email}", request.Email);
        }
        public async Task<bool> UpdatePassword(string email, string newPassword)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email, DbType.String);
            parameters.Add("@NewPassword", EncryptionService.Encrypt(newPassword), DbType.String);

            try
            {
                await connection.ExecuteAsync(
                    "UpdatePasswordByEmailOrUsername",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return true;
            }
            catch (SqlException ex)
            {
                // Log exception here if needed
                Console.WriteLine("Error updating password: " + ex.Message);
                return false;
            }
        }
        private async Task SendOtpEmailAsync(RegisterRequest request, string otp, HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var browserName = _Helper.GetBrowserInfo(userAgent);
            string[] strArrayMacIDAndIP = _Helper.GetMacIDAndIPAddress();

            TokenModel objToken = new TokenModel
            {
                EmailID = request.Email,
                OTP = otp,
                FirstName = request.FirstName,
                UserEmail = request.Email,
                IPAddress = ipAddress ?? string.Empty,
                MacAddress = strArrayMacIDAndIP[0],
                BrowserName = browserName.Name,
                BrowserVersion = browserName.Version
            };

            _Email.SendEmailFromTemplate(objToken, CommunicationTemplate.AuthenticationCode);
            _logger.LogInformation("OTP email sent successfully to {Email}", request.Email);
        }
    }
}
