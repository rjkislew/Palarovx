namespace Server.Palaro2026.DTO;

public class SportsDTO
{
    public class s_CategoriesDTO
    {
        public int? ID { get; set; }
        public string? Category { get; set; }
    }

    public class s_LevelsDTO
    {
        public int? ID { get; set; }
        public string? Level { get; set; }
    }

    public class s_SportsDTO
    {
        public int? ID { get; set; }
        public string? Sport { get; set; }
        public string? Description { get; set; }
    }

    public class s_GendersDTO
    {
        public int? ID { get; set; }
        public string? GenderCategory { get; set; }
    }

    public class s_SubCategoriesDTO
    {
        public int? ID { get; set; }
        public string? SubCategory { get; set; }
    }
}
