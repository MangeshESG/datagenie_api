namespace datagenie_api.Model.Dto
{
    public class AllMasterContactDto
    {
        public int ContactId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}