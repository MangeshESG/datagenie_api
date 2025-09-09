using datagenie_api.Interfaces;
using datagenie_api.Model;
using datagenie_api.Model.Dto;
using datagenie_api.Repositorys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace datagenie_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndustryController : ControllerBase
    {
        private readonly IIndustryRepository _repository;
        private readonly ICompanyRepository _company;

        public IndustryController(IIndustryRepository repository, ICompanyRepository company)
        {
            _repository = repository;
            _company = company;
        }

        [HttpGet("directory")]
        public async Task<IActionResult> GetIndustryDirectory([FromQuery] long currentCompanyId, [FromQuery] string categoryId = "")
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                Random random = new Random();
                categoryId = random.Next(1, 88).ToString();
            }

            var categoryName = _repository.GetCategoryNameById(Convert.ToInt32(categoryId));
            var contacts = _repository.FilterContacts(categoryId);
            var categories = await _company.GetIndustryCategoriesAsync(); // ✅ awaited
            var company = await _repository.GetIndustryCompany(categoryId, currentCompanyId); // ✅ awaited

            var viewModel = new CompaniesAndCategoriesViewModel
            {
                IndustryCategories = categories,
                Contacts = contacts,
                Company = company
            };

            return Ok(new
            {
                Category = categoryName,
                CategoryId = categoryId,
                Data = viewModel
            });
        }

    }

}
