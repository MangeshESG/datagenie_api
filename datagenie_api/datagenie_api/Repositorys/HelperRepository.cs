using datagenie_api.Interfaces;
using datagenie_api.Model.Dto;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace datagenie_api.Repositorys
{
    public class HelperRepository : IHelperRepository
    {
        public string GenerateSecureOtp(int length = 6)   
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        

        public BrowserInfo GetBrowserInfo(string userAgent)
        {
            userAgent = userAgent.ToLower();
            var browser = new BrowserInfo { Name = "Unknown", Version = "" };

            if (userAgent.Contains("edg/"))
            {
                browser.Name = "Edge";
                browser.Version = System.Text.RegularExpressions.Regex.Match(userAgent, @"edg/([\d\.]+)").Groups[1].Value;
            }
            else if (userAgent.Contains("chrome/") && !userAgent.Contains("edg/"))
            {
                browser.Name = "Chrome";
                browser.Version = System.Text.RegularExpressions.Regex.Match(userAgent, @"chrome/([\d\.]+)").Groups[1].Value;
            }
            else if (userAgent.Contains("firefox/"))
            {
                browser.Name = "Firefox";
                browser.Version = System.Text.RegularExpressions.Regex.Match(userAgent, @"firefox/([\d\.]+)").Groups[1].Value;
            }
            else if (userAgent.Contains("safari/") && !userAgent.Contains("chrome/"))
            {
                browser.Name = "Safari";
                browser.Version = System.Text.RegularExpressions.Regex.Match(userAgent, @"version/([\d\.]+)").Groups[1].Value;
            }
            else if (userAgent.Contains("opera") || userAgent.Contains("opr/"))
            {
                browser.Name = "Opera";
                browser.Version = System.Text.RegularExpressions.Regex.Match(userAgent, @"(opera|opr)/([\d\.]+)").Groups[2].Value;
            }

            return browser;
        }


        public string[] GetMacIDAndIPAddress()
        {
            string[] strArrayMacIDAndIP = new string[2];
            var macAddr =
              (
                  from nic in NetworkInterface.GetAllNetworkInterfaces()
                  where nic.OperationalStatus == OperationalStatus.Up
                  select nic.GetPhysicalAddress().ToString()
              ).FirstOrDefault();
            strArrayMacIDAndIP[0] = macAddr;

            string strIpAddress = "";

            var httpContext = new HttpContextAccessor().HttpContext;
            if (httpContext != null)
            {
                strIpAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(strIpAddress))
                {
                    strIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                }
            }

            strArrayMacIDAndIP[1] = strIpAddress;

            return strArrayMacIDAndIP;
        }
    
        // ✅ Best practice: isko interface ke andar nahi, bahar rakho
        public enum CommunicationTemplate
        {
            AuthenticationCode = 1,
            ResetPassword = 2,
            FreeTrialEmail = 3,
            RecruitmentOTP = 4,
            CompanyNotListedInRelatedContact = 5,
            RecruitmentNewOTP = 6,
            RecruitmentAckMail = 7,
            RecruitmentApprovalMail = 8,
            ExtensionLoginOTP = 9,
            RecruitmentApproveRejectMail = 10,
            ResetPasswordFromOtp = 13
        }
    }
}
