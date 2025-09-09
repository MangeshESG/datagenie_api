using datagenie_api.Interfaces;
using datagenie_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace datagenie_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly IFilterRepository _Filter;
        private readonly IContactRepository _contact;
        private readonly ICompanyRepository _company;
        public FilterController(IFilterRepository filter, IContactRepository contact, ICompanyRepository company)
        {
            _Filter = filter;
            _contact = contact;
            _company = company;
        }

        [HttpGet("GetLeadsByKeyword")]
        public async Task<IActionResult> GetLeadsByKeyword(string keyword = "", string location = "")
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                keyword = "A";
            }

            object contacts;

            if (string.IsNullOrWhiteSpace(location))
            {
                contacts = await _Filter.GetLeadsByLetter(keyword); // returns List<LatterFilterContacts>
            }
            else
            {
                contacts = await _contact.GetContactsByLocationAsync(location); // returns List<Contacts>
            }


            var companies = await _company.GetRandomCompaniesAsync();
            var industries = await _company.GetIndustryCategoriesAsync();

            var result = new
            {
                Contacts = contacts,
                Companies = companies,
                Industries = industries
            };

            return Ok(result);
        }

    }
}
