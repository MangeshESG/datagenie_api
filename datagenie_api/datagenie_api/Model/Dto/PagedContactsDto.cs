namespace datagenie_api.Model.Dto
{
    // Models/Dto/PagedContactsDto.cs
    public class PagedContactsDto
    {
        public List<ContactDto> Contacts { get; set; }
        public int TotalCount { get; set; }
    }

}
