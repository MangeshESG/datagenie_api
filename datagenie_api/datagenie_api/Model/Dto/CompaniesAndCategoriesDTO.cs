namespace datagenie_api.Model.Dto
{
    public class CompaniesAndCategoriesDTO
    {
        public List<Company> Companies { get; set; }
        public List<IndustryCategories> IndustryCategories { get; set; }

        public List<Contacts> Contacts { get; set; }
    }
}
