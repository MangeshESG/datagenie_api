using System.Text.Json.Serialization;

namespace datagenie_api.Model.Dto
{
    public class ValidUser
    {
        public string Email { get; set; }
        public int Count { get; set; }
        public string Username { get; set; }
    }
}
