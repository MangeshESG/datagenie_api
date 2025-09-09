using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using datagenie_api.Data;
using Dapper;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using datagenie_api.Model.Dto;
using datagenie_api.Interfaces;
using Newtonsoft.Json.Linq;
using static datagenie_api.Repositorys.HelperRepository;
using datagenie_api.Model;
using datagenie_api.Services;
using Azure.Core;
using System.Net.Http;
using System.Data;
using static System.Net.WebRequestMethods;

namespace datagenie_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DapperContext _context;
        private readonly IHelperRepository _Helper;
        private readonly IEmailService _Email;
        private readonly IRegistrationRepository _registration;
        private readonly JwtService _jwtService; // Add an instance of JwtService  

        public AuthController(DapperContext context, IHelperRepository helper, IEmailService email, IRegistrationRepository registration, JwtService jwtService)
        {
            _context = context;
            _Helper = helper;
            _Email = email;
            _registration = registration;
            _jwtService = jwtService; // Initialize the JwtService instance  
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _registration.RegisterEmail(request, HttpContext);
            return Ok(response);
        }



        [HttpPost("registration-verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromQuery] string Email, [FromQuery] string Otp)
        {
            // 1. Check OTP
            var otpRecord = await _registration.GetOtpRecordAsync(Email, Otp);

            if (otpRecord == null)
                return BadRequest("Invalid OTP.");

            if (otpRecord.ExpiresAt < DateTime.Now)
                return BadRequest("OTP expired.");

            // 2. Mark OTP as verified
            await _registration.MarkOtpVerifiedAsync(Email, Otp);

            // 3. Get temporary registration data
            var tempData = await _registration.GetTempRegisterDataAsync(Email);

            if (tempData == null)
                return BadRequest("Registration data expired. Please register again.");

            var requestData = JsonConvert.DeserializeObject<RegisterRequest>(tempData.JsonData);

            // 4. Call  USP_RegisterNewUser
            var response = await _registration.VerifyAndRegisterUserAsync(requestData);

            // 5. Cleanup temp data
            await _registration.DeleteTempRegisterDataAsync(Email);

            return Ok(response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery] string Username, [FromQuery] string Password, [FromQuery] int? trustednumber)
        {
            // 1. Check user credentials using stored procedure
            var validation = await _registration.CheckUserExists(Username, EncryptionService.Encrypt(Password), 1);

            if (validation == null)
                return Unauthorized(new { Message = "Invalid credentials" });

            // 2. If trusted device matches → direct JWT token
            if (!string.IsNullOrEmpty(validation.TrustedDeviceNumber) && validation.TrustedDeviceNumber == trustednumber?.ToString())
            {
                var tokenDirect = _jwtService.GeneratenewToken(Username, validation.ClientID, validation.FirstName);
                return Ok(new { Token = tokenDirect });
            }

            // 3. Generate OTP for login if device not trusted
            string otp = _Helper.GenerateSecureOtp();

            // 4. Send OTP email
            _registration.SendLoginOtpEmail(validation.Email, validation.FirstName, otp, HttpContext);

            // 5. Insert OTP record into DB
            var otpEntity = new
            {
                Email = validation.Email,
                Username = validation.UserName,
                OTP = otp,
                IsVerified = false,
                CreatedAt = DateTime.Now,
                OtpType = "login",
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "sp_InsertEmailOtpVerification",
                otpEntity,
                commandType: CommandType.StoredProcedure
            );

            return Ok(new
            {
                success = true,
                message = "OTP sent successfully"
            });
        }


        [HttpPost("verify_trust_otp")]
        public async Task<IActionResult> TrustedDivice([FromQuery] string? username, [FromQuery] string otp, [FromQuery] bool trustthisdivice, [FromQuery] string Password)
        {
            var validation = await _registration.CheckUserExists(username, EncryptionService.Encrypt(Password), 1);

            var otpDetails = await _registration.GetOtpRecordAsync(username, otp);


            if (string.IsNullOrEmpty(otp) ||
                validation == null ||
                otpDetails == null ||
                otpDetails.OTP != otp ||
                otpDetails.ExpiresAt < DateTime.Now ||
                otpDetails.IsVerified)
            {
                return BadRequest("Invalid OTP, try again");
            }

            await _registration.MarkOtpVerifiedAsync(username, otp);

            if (trustthisdivice)
            {
                Random rnd = new Random();
                int trustedCode = rnd.Next(100000, 999999);  
                validation.TrustedDeviceNumber = trustedCode.ToString();
                await _registration.UpdateTrustedDeviceNumberAsync(validation.ClientID, trustedCode.ToString());

            }

            var token = _jwtService.GeneratenewToken(username, validation.ClientID, validation.FirstName.ToString());
            return Ok(new
            {
                Token = token,
                trustenumber = validation.TrustedDeviceNumber,
                //ClientID = user.ClientID,
                //Isadmin = user.IsAdmin,
                //IsDemoAccount = user.IsDemoAccount,
                //FirstName = user.FirstName,
                //LastName = user.LastName,
                //CompanyName = user.CompanyName,
            });
        }

        [HttpPost("restpass_send-otp")]
        public async Task<IActionResult> SendResetPasswordOtp([FromQuery] string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
                return BadRequest(new { message = "Email is required." });

            var user = await _registration.CheckClientExists(Email);

            if (user.Count == 0)
                return NotFound(new { message = "Client not found with the given email or username." });

            string otp = _Helper.GenerateSecureOtp();

            var otpEntity = new
            {
                Email = Email,
                Username = user.Username,
                OTP = otp,
                IsVerified = false,
                CreatedAt = DateTime.Now,
                OtpType = "reset_password",
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };

            using (var connection = _context.CreateConnection()) // Ensure connection is used correctly here
            {
                await _registration.InsertOtpRecordAsync(connection, Email, user.Username, otp);
            }
            TokenModel objToken = new TokenModel();
            objToken.EmailID = Email;
            objToken.UserEmail = Email;
            objToken.OTP = otp;

            await _Email.SendEmailFromTemplate(objToken, CommunicationTemplate.ResetPasswordFromOtp);
            return Ok(new
            {
                success = true,
                message = "OTP sent successfully"
            });
        }

        [HttpPost("restpassword-verify-otp")]
        public async Task<IActionResult> ResetPassword([FromQuery] string Email, [FromQuery] string Otp, [FromQuery] string newPassword)
        {
            // 1. Check OTP
            var otpRecord = await _registration.GetOtpRecordAsync(Email, Otp);

            if (otpRecord == null)
                return BadRequest("Invalid OTP.");

            if (otpRecord.ExpiresAt < DateTime.Now)
                return BadRequest("OTP expired.");

            // 2. Mark OTP as verified
            await _registration.MarkOtpVerifiedAsync(Email, Otp);

            // 3. Get temporary registration data
            var response = await _registration.UpdatePassword(Email, newPassword);

            // 5. Cleanup temp data

            return Ok(response);
        }
    }
}
