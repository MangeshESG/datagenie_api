using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using datagenie_api.Model.Dto;
using datagenie_api.Model.Dto.datagenie_api.Model.Dto;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

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
            var result = await connection.QueryAsync<FilterContacts>(
                "GetContactsByLocation",
                new { Location = location },
                commandType: CommandType.StoredProcedure
            );

            // Map FilterContacts to Contacts  
            var contacts = result.Select(fc => new Contacts
            {
                RECORD_ID = fc.RECORD_ID,
                CONTACT = fc.CONTACT,
                COMPANY = fc.COMPANY,
                PHONE = fc.PHONE,
                Email_id = fc.Email_id,
                CONTACT_IMAGE_URL = fc.CONTACT_IMAGE_URL,
                Country = fc.LOCATION,
                JobTitle = fc.Title
            }).ToList();

            return contacts;
        }

        public async Task<PagedContactsDto> GetRelatedContactsWithCount(decimal masterCompanyRecordId, int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();
            var result = new PagedContactsDto();
            using (var multi = await connection.QueryMultipleAsync(
                "RelatedContactsForIndexingWithCount",
                new
                {
                    MasterCompanyRecordId = masterCompanyRecordId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure))
            {
                result.Contacts = (await multi.ReadAsync<ContactDto>()).ToList();
                result.TotalCount = await multi.ReadFirstAsync<int>();
            }

            return result;
        }
        public async Task<List<MasterContactDto>> GetMasterContactsByTitle(string title)
        {
            using var connection = _context.CreateConnection();

            var contacts = (await connection.QueryAsync<MasterContactDto>(
                "GetMasterContactsByTitle",
                new { Title = title },
                commandType: CommandType.StoredProcedure)).ToList();

            return contacts;
        }

        public ContactsVM GetContactDetails(long recordIdLong, string flag)
        {

            using (var conn = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@RecordID", recordIdLong);
                p.Add("@flag", 1);

                var result = conn.QueryFirstOrDefault<ContactsVM>("USP_GetContactDetails", p, commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                    result.CONTACT = Regex.Replace(result.CONTACT ?? "", @"[^0-9a-zA-Z ]+", "");
                    result.TITLE = Regex.Replace(result.TITLE ?? "", @"[^0-9a-zA-Z ]+", "");
                    result.MetaDescription = GetContactDescription(result.TITLE);

                    if (ImageExists(result.CONTACT_GUID.ToString()))
                    {
                        result.CONTACT_IMAGE_URL = "https://searchgenie.e-s-g.co.uk/media/contacts/" + result.CONTACT_GUID + ".jpg";
                    }
                }

                return result;
            }
        }

        public string GetContactDescription(string inputTitle)
        {
            using (var conn = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@input_title", inputTitle, DbType.String, size: 150);
                p.Add("@output_description", dbType: DbType.String, size: 1000, direction: ParameterDirection.Output);

                conn.Execute("get_occupation_description", p, commandType: CommandType.StoredProcedure);

                return p.Get<string>("@output_description");
            }
        }

        public List<long> GetMatchingContactRecordIds(string name, string company, string industry)
        {
            using (var conn = _context.CreateConnection())
            {
                var sql = @"
                SELECT RECORD_ID 
                FROM MASTERCONTACTS 
                WHERE CONTACT = @Name AND COMPANY = @CompanyName AND INDUSTRY = @Industry 
                ORDER BY CAST(RECORD_ID AS BIGINT) ASC";

                return conn.Query<long>(sql, new { Name = name, CompanyName = company, Industry = industry }).ToList();
            }
        }

        public string GetGender(string contactFirstName)
        {
            using (var conn = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ContactName", contactFirstName);

                var data = conn.QueryFirstOrDefault<ContactGender>("GetGenderCategory", p, commandType: CommandType.StoredProcedure);
                return data?.Gender;
            }
        }

        public bool IsEmailUnlocked(string recordId, int clientId)
        {
            using (var conn = _context.CreateConnection())
            {
                var sql = "SELECT 1 FROM UnlockedEmails WHERE RecordID = @RecordId AND ClientID = @ClientID";
                var result = conn.ExecuteScalar<int?>(sql, new { RecordId = recordId, ClientID = clientId });
                return result.HasValue;
            }
        }

        public bool ImageExists(string guid)
        {
            var path = Path.Combine("D:\\Plesk\\VHOSTS\\searchgenie.e-s-g.co.uk\\httpdocs\\media\\contacts", guid + ".jpg");
            return File.Exists(path);
        }

    }

}
