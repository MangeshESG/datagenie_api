using datagenie_api.Model;
using datagenie_api.Model.Dto;
using datagenie_api.Model.Dto.datagenie_api.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace datagenie_api.Interfaces
{
    public interface IContactRepository
    {
        Task<List<Contacts>> GetRandomContactsAsync();
        Task<List<Contacts>> GetContactsByLocationAsync(string location);
        Task<PagedContactsDto> GetRelatedContactsWithCount(decimal masterCompanyRecordId, int pageNumber, int pageSize);
        Task<List<MasterContactDto>> GetMasterContactsByTitle(string title);
        ContactsVM GetContactDetails(long recordIdLong, string flag);
        string GetContactDescription(string inputTitle);
        List<long> GetMatchingContactRecordIds(string name, string company, string industry);
        string GetGender(string contactFirstName);
        bool IsEmailUnlocked(string recordId, int clientId);
        bool ImageExists(string guid);
    }
}
