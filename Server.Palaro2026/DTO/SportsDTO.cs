using Server.Palaro2026.Entities;

namespace Server.Palaro2026.DTO;

public class SportsDTO
{
    public class SportCategories
    {
        public int ID { get; set; }
        public string? Category { get; set; }
    }

    public class Sports
    {
        public int ID { get; set; }
        public string? Sport { get; set; }
        public string? Description { get; set; }
        public int? SportCategoryID { get; set; }
    }

    public class SportSubcategories
    {
        public int ID { get; set; }
        public string? Subcategory { get; set; }
        public int? SportID { get; set; }
        public int? SportGenderCategoryID { get; set; }
        public int? SchoolLevelID { get; set; }
    }

    public class SportGenderCategories
    {
        public int ID { get; set; }
        public string? Gender { get; set; }
    }
}
