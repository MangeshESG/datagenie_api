using Microsoft.Data.SqlClient;
using System.Data;

namespace datagenie_api.Data   // ✅ namespace yaha "Data" rakho, "DapperContext" nahi
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
