using datagenie_api.Model;

namespace datagenie_api.Interfaces
{
    public interface IIndustryRepository
    {
        string GetCategoryNameById(int categoryId);

        List<ContactIndustryVM> FilterContacts(string categoryId);

        Task<List<CompanyIndustryVM>> GetIndustryCompany(string categoryId, long currentCompanyId);
    }
}
