namespace Server.Palaro2026.DTO;

public class SportCategoriesSubCategoriesDTO
{
    public class scsc_CategoriesDTO
    {
        public string? category { get; set; }
        public List<scsc_SportsDTO>? sports { get; set; }
    }

    public class scsc_SportsDTO
    {
        public string? sport { get; set; }
        public string? description { get; set; }
        public List<scsc_SubCategoriesDTO>? sub_categories { get; set; }
    }

    public class scsc_SubCategoriesDTO
    {
        public string? sub_category { get; set; }
    }
}