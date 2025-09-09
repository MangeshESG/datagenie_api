namespace datagenie_api.Model
{
    public class ContactIndustryVM
    {
        public long RECORD_ID { get; set; }          // Contact ka unique ID
        public string CONTACT { get; set; }            // Contact ka naam
        public string COMPANY { get; set; }            // Company ka naam
        public string TITLE { get; set; }            // Company ka naam
        public string LOCATION { get; set; }            // Company ka naam
        public string WEBSITE { get; set; }            // Company ka naam
        public string HUNTER_EMAIL { get; set; }
        public string OTHER_DETAILS { get; set; }
        public string CACHE_ALL_FALSE_ACCEPTED { get; set; }
        public string mastercompanyrecord_id { get; set; }
        public string company_image_uplead_url { get; set; }
        public Guid CONTACT_GUID { get; set; }
        public string COMPANY_GUID { get; set; }
        public string LinkedInURL { get; set; }
        public string INDUSTRY { get; set; }
        public string GENDER { get; set; }
        public string CONTACT_IMAGE_URL { get; set; }
        public string PHONE { get; set; }
    }
    public class CompanyIndustryVM
    {
        public long RECORD_ID { get; set; }          
        public string COMPANY_NAME { get; set; }          
        public string SLAES { get; set; }          
        public int Categoryid { get; set; }          
        public string company_image_url { get; set; } 
        
    }
    public class CompaniesAndCategoriesViewModel
    {
        public List<IndustryCategories> IndustryCategories { get; set; }
        public List<ContactIndustryVM> Contacts { get; set; }
        public List<CompanyIndustryVM> Company { get; set; }
    }

}
