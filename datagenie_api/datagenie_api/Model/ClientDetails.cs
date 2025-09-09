namespace datagenie_api.Model
{
    public class ClientDetails
    {
        public int ClientID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsClient { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal Credit { get; set; }
        public string? TrustedDeviceNumber { get; set; } = string.Empty;
        public string ProductsAndServices { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShowAnalytics { get; set; } = "0";

        // Note: Version 1 returns 'UserTypes', Version 2 returns 'UserType'
        public string UserTypes { get; set; } = string.Empty;
    }

}
