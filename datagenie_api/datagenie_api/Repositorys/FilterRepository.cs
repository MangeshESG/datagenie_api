using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using System.Data;

namespace datagenie_api.Repositorys
{
    public class FilterRepository : IFilterRepository
    {
        private readonly DapperContext _context;
        public FilterRepository(DapperContext context)
        {
            _context = context;
        }


        public async Task<List<LatterFilterContacts>> GetLeadsByLetter(string KeyWord)
        {
            using var connection = _context.CreateConnection();

            var contacts = (await connection.QueryAsync<LatterFilterContacts>(
                "GetContactDirectory",
                new { keyword = KeyWord },
                commandType: CommandType.StoredProcedure)).ToList();

            return contacts;
        }
    }
}
