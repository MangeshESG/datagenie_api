using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using datagenie_api.Model.Dto;
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

        public async Task<CompanyVM> GetCompanyByRecordId(decimal recordId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@recordID", recordId);

            var result = await connection.QueryFirstOrDefaultAsync<CompanyVM>(
                "usp_ExtentionGetCompanyDatawithrecordId",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<List<long>> GetMatchingCompanyRecordIds(string companyName, string industry, string website)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
            SELECT RECORD_ID 
            FROM MASTERCOMPANY 
            WHERE COMPANY_NAME = @CompanyName 
                AND INDUSTRY = @Industry 
                AND WEBSITE = @Website 
            ORDER BY CAST(RECORD_ID AS BIGINT) ASC";

            var result = await connection.QueryAsync<long>(sql, new
            {
                CompanyName = companyName.Trim(),
                Industry = industry.Trim(),
                Website = website.Trim()
            });

            return result.ToList();
        }
        public async Task<List<CompanyDto>> GetSimilarCompanies(string categoryId, decimal currentCompanyId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Category", categoryId);
            parameters.Add("@CurrentCompanyId", currentCompanyId);

            var companies = await connection.QueryAsync<CompanyDto>(
                "usp_GetSimilarCompanies",
                parameters,
                commandType: CommandType.StoredProcedure);

            return companies.ToList();
        }
    }

}
