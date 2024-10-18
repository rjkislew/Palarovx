namespace Server.Palaro2026.DTO;

public class SportCategoriesDTO
{
    public class sc_CategoriesDTO
    {
        public string? Category { get; set; }
        public List<sc_SportsDTO>? Sports { get; set; }
    }

    public class sc_SportsDTO
    {
        public string? Sport { get; set; }
        public string? Description { get; set; }
    }
}