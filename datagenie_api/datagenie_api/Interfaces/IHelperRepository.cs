using datagenie_api.Model.Dto;

namespace datagenie_api.Interfaces
{
    public interface IHelperRepository
    {
        string GenerateSecureOtp(int length = 6);
        BrowserInfo GetBrowserInfo(string userAgent);
        string[] GetMacIDAndIPAddress();
        
    }
}
