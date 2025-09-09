using datagenie_api.Model;
using datagenie_api.Model.Dto;
using System.Data;
using System.Threading.Tasks;

namespace datagenie_api.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<string> VerifyAndRegisterUserAsync(RegisterRequest request);
        Task<dynamic> GetOtpRecordAsync(string email, string otp);
        Task MarkOtpVerifiedAsync(string email, string otp);
        Task<dynamic> GetTempRegisterDataAsync(string email);
        Task DeleteTempRegisterDataAsync(string email);
        Task<string> RegisterEmail(RegisterRequest request, HttpContext httpContext);
        Task<ClientDetails?> CheckUserExists(string username, string password, int version = 1);
        Task<ValidUser> CheckClientExists(string email, string versionId = "V1");
        Task SendLoginOtpEmail(string Email, string FirstName, string otp, HttpContext httpContext);
        Task<TrustedDeviceResponse> UpdateTrustedDeviceNumberAsync(int clientId, string trustedNumber);
        Task InsertTempRegisterDataAsync(IDbConnection connection, RegisterRequest request);
        Task InsertOtpRecordAsync(IDbConnection connection, string? Email, string? username, string otp);
        Task<bool> UpdatePassword(string email, string newPassword);
        
            //Task<string?> GetPassword(string username);
    }

}
