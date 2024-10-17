namespace Server.Palaro2026.DTO;

public class SportCategoriesLevelsGendersSubCategoriesDTO
{
    public class sclgsc_CategoriesDTO
    {
        public string? category { get; set; }
        public List<sclgsc_LevelsDTO>? levels { get; set; }
    }

    public class sclgsc_LevelsDTO
    {
        public string? level { get; set; }
        public List<sclgsc_SportsDTO>? sports { get; set; }
    }

    public class sclgsc_SportsDTO
    {
        public string? sport { get; set; }
        public string? description { get; set; }
        public List<sclgsc_GendersDTO>? gender_categories { get; set; }
    }

    public class sclgsc_GendersDTO
    {
        public string? gender_category { get; set; }
        public List<sclgsc_SubCategoriesDTO>? sub_categories { get; set; }
    }

    public class sclgsc_SubCategoriesDTO
    {
        public string? sub_category { get; set; }
    }
}