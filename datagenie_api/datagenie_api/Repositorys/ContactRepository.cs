using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using Microsoft.Data.SqlClient;
using System.Data;

namespace datagenie_api.Repositorys
{
    public class ContactRepository : IContactRepository
    {
        private readonly DapperContext _context;

        public ContactRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<Contacts>> GetRandomContactsAsync()
        {
            List<string> titles = new List<string> { "Chief", "Senior", "Vice", "Manger", "cto", "cfo", "advisor" };
            Random rand = new Random();
            string randomTitle = titles[rand.Next(0, titles.Count)];
            using var connection = _context.CreateConnection();

            var result = await connection.QueryAsync<Contacts>(
                "GetRandomContactDetails",
                new { title = randomTitle },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<Contacts>> GetContactsByLocationAsync(string location)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<Contacts>(
                "GetContactsByLocation",
                new { Location = location },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
    }

}
