using datagenie_api.Interfaces;
using datagenie_api.Model.Dto;
using datagenie_api.Repositorys;
using datagenie_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace datagenie_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly ICompanyRepository _company;
        private readonly IContactRepository _contact;

        public CommonController(ICompanyRepository company, IContactRepository contact)
        {
            _company = company;
            _contact = contact;
        }

        [HttpGet("GetCompanyDirectory")]
        public async Task<IActionResult> GetCompanyDirectory()
        {
            var companies = await _company.GetRandomCompaniesAsync();
            var categories = await _company.GetIndustryCategoriesAsync();

            var result = new CompaniesAndCategoriesDTO
            {
                Companies = companies,
                IndustryCategories = categories
            };

            return Ok(result);
        }

        [HttpGet("ContactDirectory")]
        public async Task<IActionResult> GetContactDirectory([FromQuery] string location = "")
        {
            var contacts = string.IsNullOrEmpty(location)
                ? await _contact.GetRandomContactsAsync()
                : await _contact.GetContactsByLocationAsync(location);

            var categories = await _company.GetIndustryCategoriesAsync();

            var viewModel = new CompaniesAndCategoriesDTO
            {
                Contacts = contacts,
                IndustryCategories = categories
            };

            return Ok(viewModel);
        }

        [HttpGet("Company-info{recordId}")]
        public async Task<IActionResult> GetCompany(decimal recordId)
        {
            var company = await _company.GetCompanyByRecordId(recordId);
            if (company == null)
                return NotFound("Company not found");

            // Check if multiple companies exist with same Name, Industry, Website
            if (!string.IsNullOrEmpty(company.COMPANY_NAME) &&
                !string.IsNullOrEmpty(company.INDUSTRY) &&
                !string.IsNullOrEmpty(company.WEBSITE))
            {
                var recordIds = await _company.GetMatchingCompanyRecordIds(
                    company.COMPANY_NAME, company.INDUSTRY, company.WEBSITE);

                if (recordIds.Count > 1)
                {
                    var smallestId = recordIds.Min();
                    if (smallestId != (long)recordId)
                    {
                        return RedirectPermanent($"/api/company/{smallestId}");
                    }
                }
            }

            return Ok(company);
        }

        [HttpGet("similar-companies/{currentCompanyId}")]
        public async Task<IActionResult> GetSimilarCompanies(decimal currentCompanyId, [FromQuery] string? categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                var random = new Random();
                int randomNumber = random.Next(1, 88);
                categoryId = randomNumber.ToString();
            }

            var companies = await _company.GetSimilarCompanies(categoryId, currentCompanyId);

            return Ok(companies);
        }

        [HttpGet("related-contact")]
        public async Task<IActionResult> GetRelatedContactsWithCount(
       [FromQuery] decimal masterCompanyRecordId,
       [FromQuery] int pageNumber = 1,
       [FromQuery] int pageSize = 10)
        {
            if (masterCompanyRecordId <= 0)
                return BadRequest("Invalid MasterCompanyRecordId");

            var data = await _contact.GetRelatedContactsWithCount(masterCompanyRecordId, pageNumber, pageSize);
            return Ok(data);
        }


        [HttpGet("contact/info")]
        [AllowAnonymous] // ✅ Public access allowed
        public IActionResult GetContactInfo(string name, string recordId, string? flag)
        {
            if (string.IsNullOrEmpty(recordId) || !long.TryParse(recordId, out long recordIdLong))
                return BadRequest("Invalid recordId");

            var contact = _contact.GetContactDetails(recordIdLong, flag);

            if (contact == null)
                return NotFound("Contact not found");

            var firstName = contact.CONTACT?.Split(' ')[0];
            if (!string.IsNullOrEmpty(firstName))
            {
                contact.Gender = _contact.GetGender(firstName);
            }

            // ✅ ClientID optional — no JWT required
            string? clientIdStr = User?.Claims?.FirstOrDefault(c => c.Type == "ClientID")?.Value;

            if (!string.IsNullOrEmpty(clientIdStr) && int.TryParse(clientIdStr, out int clientId))
            {
                bool isUnlocked = _contact.IsEmailUnlocked(recordId, clientId);
                contact.displayEmail = isUnlocked ? contact.HUNTER_EMAIL : string.Empty;
            }
            else
            {
                contact.displayEmail = string.Empty; // 🔐 Hide if not logged in
            }

            return Ok(contact);
        }

        [HttpGet("GetByTitle")]
        public async Task<IActionResult> GetContactsByTitle(string title)
        {
            var result = await _contact.GetMasterContactsByTitle(title);
            return Ok(result);
        }

    }

}
