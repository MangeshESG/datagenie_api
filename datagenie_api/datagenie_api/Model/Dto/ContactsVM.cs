using System.ComponentModel.DataAnnotations.Schema;

namespace datagenie_api.Model.Dto
{
    using System.ComponentModel.DataAnnotations.Schema;

    namespace datagenie_api.Model.Dto
    {
        public class ContactsVM
        {
            public string RECORD_ID { get; set; }          // Contact ka unique ID
            public string CONTACT { get; set; }            // Contact ka naam
            public string COMPANY { get; set; }            // Company ka naam
            public string TITLE { get; set; }              // Contact ka designation / job title
            public string CONTACTLOCATION { get; set; }              // Phone number
            public string LOCATION { get; set; }           // Company location
            public string INDUSTRY { get; set; }           // Industry type
            public string CONTACT_IMAGE_URL { get; set; }  // Contact ki image ka URL
            public string CONTACT_URL { get; set; }        // Contact profile ka URL
            public string COMPANY_URL { get; set; }        // Company ka profile URL
            public string File_ID { get; set; }            // File identifier
            public string HUNTER_EMAIL { get; set; }       // Email from Hunter API
            public string OTHER_DETAILS { get; set; }      // Additional info
            public string CACHE_ALL_FALSE_ACCEPTED { get; set; }  // Cached result flag
            public string SMTP_DETAILS { get; set; }       // SMTP validation result
            public Guid CONTACT_GUID { get; set; }       // Unique GUID for contact
            public string LinkedInUrl { get; set; }        // LinkedIn URL
            public string MasterCompanyRecord_Id { get; set; }  // FK to company

            public string MetaDescription { get; set; }    // Company Meta Description
            public string PHONE { get; set; }    // Company Meta Description
            [Column("SIC Code")]
            public string SICCode { get; set; }            // SIC Code
            [Column("NAICS Code")]
            public string NAICSCode { get; set; }          // NAICS Code
            public string Revenue { get; set; }            // Company revenue
            public string Employees { get; set; }          // No. of employees
            public string WEBSITE { get; set; }            // Company website
            [Column("Company Related URL's")]
            public string CompanyRelatedURLs { get; set; } // Extra URLs related to company
            public string Description { get; set; }        // Company description
            public string Gender { get; set; }
            //[NotMapped]// Gender (only in TEMP table)
            public string displayEmail { get; set; }             // Gender (only in TEMP table)
        }
    }

}
