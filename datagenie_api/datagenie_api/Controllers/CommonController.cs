using datagenie_api.Interfaces;
using datagenie_api.Model.Dto;
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
    }

}
