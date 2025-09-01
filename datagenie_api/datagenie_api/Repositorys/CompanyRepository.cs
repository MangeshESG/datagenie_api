using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using Microsoft.Data.SqlClient;
using System.Data;

namespace datagenie_api.Repositorys
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DapperContext _context;

        public CompanyRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetRandomCompaniesAsync()
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<Company>(
                "GetRandomMasterCompanies",
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<IndustryCategories>> GetIndustryCategoriesAsync()
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<IndustryCategories>(
                "GetRandomCompanyCategories",
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
    }

}
