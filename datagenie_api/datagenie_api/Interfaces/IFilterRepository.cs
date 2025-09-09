using datagenie_api.Model;

namespace datagenie_api.Interfaces
{
    public interface IFilterRepository
    {
        Task<List<LatterFilterContacts>> GetLeadsByLetter(string KeyWord);
    }
}
