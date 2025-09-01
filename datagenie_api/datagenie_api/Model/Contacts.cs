using System.ComponentModel.DataAnnotations.Schema;

namespace datagenie_api.Model
{
    public class Contacts
    {
        public long RECORD_ID { get; set; }
        public string CONTACT { get; set; }
        public string COMPANY { get; set; }
        public string PHONE { get; set; }
        public string Email_id { get; set; }
        public string CONTACT_IMAGE_URL { get; set; }
        public string Country { get; set; }
        public string JobTitle { get; set; }

    }
}
