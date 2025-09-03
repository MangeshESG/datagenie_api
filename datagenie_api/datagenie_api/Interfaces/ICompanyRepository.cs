using datagenie_api.Model;
using datagenie_api.Model.Dto;

namespace datagenie_api.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetRandomCompaniesAsync();
        Task<List<IndustryCategories>> GetIndustryCategoriesAsync();
        Task<CompanyVM> GetCompanyByRecordId(decimal recordId);
        Task<List<long>> GetMatchingCompanyRecordIds(string companyName, string industry, string website);
        Task<List<CompanyDto>> GetSimilarCompanies(string categoryId, decimal currentCompanyId);


    }
}
