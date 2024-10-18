namespace Server.Palaro2026.DTO;

public class SportCategoriesSubCategoriesDTO
{
    public class scsc_CategoriesDTO
    {
        public string? Category { get; set; }
        public List<scsc_SportsDTO>? Sports { get; set; }
    }

    public class scsc_SportsDTO
    {
        public string? Sport { get; set; }
        public string? Description { get; set; }
        public List<scsc_SubCategoriesDTO>? sub_categories { get; set; }
    }

    public class scsc_SubCategoriesDTO
    {
        public string? SubCategory { get; set; }
    }
}