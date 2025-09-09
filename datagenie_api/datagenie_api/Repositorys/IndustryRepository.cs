using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using datagenie_api.Model.Dto;
using datagenie_api.Model.Dto.datagenie_api.Model.Dto;
using System.Data;

namespace datagenie_api.Repositorys
{
    public class IndustryRepository : IIndustryRepository
    {
        private readonly DapperContext _Context;
        public IndustryRepository(DapperContext dapperContext)
        {
            _Context = dapperContext;
        }
        public string GetCategoryNameById(int categoryId)
        {
            using var connection = _Context.CreateConnection();
            
                string sql = "SELECT category FROM dbo.tbl_CompanyCategory WHERE categoryId = @categoryId";
                return connection.QueryFirstOrDefault<string>(sql, new { categoryId });
            
        }
        public List<ContactIndustryVM> FilterContacts(string categoryId)
        {
            using var connection = _Context.CreateConnection();
            
                var parameters = new DynamicParameters();
                parameters.Add("@Industry", categoryId);
                parameters.Add("@Subcategory", "");
                parameters.Add("@CompanyName", "");
                parameters.Add("@Position", "");
                parameters.Add("@company_url", "");
                parameters.Add("@company_revenue", "");
                parameters.Add("@Country", "");
                parameters.Add("@State_", "");
                parameters.Add("@City_", "");
                parameters.Add("@columnDatabase", "CONTACT");
                parameters.Add("@sortDirection", "asc");
                parameters.Add("@start", "0");
                parameters.Add("@length", "6");
                parameters.Add("@clientId", "1641");
                parameters.Add("@titleSearchType", "");
                parameters.Add("@RadioInculde", "");
                parameters.Add("@JobFunction", "");
                parameters.Add("@jobid", "");
                parameters.Add("@name", "");
                parameters.Add("@jobNameIncludes", "");

                var results = connection.Query<ContactIndustryVM>(
                    "USP_GetContactFilterData_071124",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return results;
            
        }

        public async Task<List<CompanyIndustryVM>> GetIndustryCompany(string categoryId, long currentCompanyId)
        {
            using var connection = _Context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Category", categoryId);
            parameters.Add("@CurrentCompanyId", currentCompanyId);

            var result = await connection.QueryAsync<CompanyIndustryVM>(
                "usp_GetSimilarCompanies",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


    }

}
