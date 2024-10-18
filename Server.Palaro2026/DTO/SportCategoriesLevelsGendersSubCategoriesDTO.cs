namespace Server.Palaro2026.DTO;

public class SportCategoriesLevelsGendersSubCategoriesDTO
{
    public class sclgsc_CategoriesDTO
    {
        public string? Category { get; set; }
        public List<sclgsc_LevelsDTO>? Levels { get; set; }
    }

    public class sclgsc_LevelsDTO
    {
        public string? Level { get; set; }
        public List<sclgsc_SportsDTO>? Sports { get; set; }
    }

    public class sclgsc_SportsDTO
    {
        public string? Sport { get; set; }
        public string? Description { get; set; }
        public List<sclgsc_GendersDTO>? GenderCategories { get; set; }
    }

    public class sclgsc_GendersDTO
    {
        public string? GenderCategory { get; set; }
        public List<sclgsc_SubCategoriesDTO>? sub_categories { get; set; }
    }

    public class sclgsc_SubCategoriesDTO
    {
        public string? SubCategory { get; set; }
    }
}