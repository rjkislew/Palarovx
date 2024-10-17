namespace Server.Palaro2026.DTO;

public class SportCategoriesDTO
{
    public class sc_CategoriesDTO
    {
        public string? category { get; set; }
        public List<sc_SportsDTO>? sports { get; set; }
    }

    public class sc_SportsDTO
    {
        public string? sport { get; set; }
        public string? description { get; set; }
    }
}