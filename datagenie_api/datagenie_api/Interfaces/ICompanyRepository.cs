using datagenie_api.Model;

namespace datagenie_api.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetRandomCompaniesAsync();
        Task<List<IndustryCategories>> GetIndustryCategoriesAsync();
    }
}
