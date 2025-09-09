namespace datagenie_api.Model
{
    public class CommunicationTemplateDetails
    {
        public string Body { get; set; }
        public string MasterHtml { get; set; }
        public string Subject { get; set; }
        public string BCC { get; set; }
        public string CC { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

}
