namespace datagenie_api.Model.Dto
{
    public class TrustedDeviceResponse
    {
        public int ClientID { get; set; }
        public string TrustedDeviceNumber { get; set; }
        public DateTime? ModifiedAccountDatetime { get; set; }
    }
}
