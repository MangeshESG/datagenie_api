using datagenie_api.Model;

namespace datagenie_api.Interfaces
{
    public interface IContactRepository
    {
        Task<List<Contacts>> GetRandomContactsAsync();
        Task<List<Contacts>> GetContactsByLocationAsync(string location);
    }

}
