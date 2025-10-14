namespace Server.Palaro2026.DTO;

public class SportsDTO
{
    public class SportDetails
    {
        public class SportCategory
        {
            public int ID { get; set; }
            public string? Category { get; set; }
            public List<Sports>? SportsList { get; set; }
        }

        public class Sports
        {
            public int ID { get; set; }
            public string? Sport { get; set; }
            public string? Description { get; set; }
            public List<SchoolLevels>? SchoolLevelsList { get; set; }
        }

        public class SchoolLevels
        {
            public int ID { get; set; }
            public string? Level { get; set; }
            public List<SportGenderCategories>? SportGenderCategoriesList { get; set; }
        }

        public class SportGenderCategories
        {
            public int ID { get; set; }
            public string? Gender { get; set; }
            public List<SportSubcategories>? SportSubcategoriesList { get; set; }
        }

        public class SportSubcategories
        {
            public int ID { get; set; }
            public string? Subcategory { get; set; }
            public string? MainCategory { get; set; }
        }
    }

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
        public string? MainCategory { get; set; }
    }

    public class SportGenderCategories
    {
        public int ID { get; set; }
        public string? Gender { get; set; }
    }


    // New bracket-level DTOs
    public class SportsBracket
    {
        public int ID { get; set; }
        public int SportID { get; set; }
        public string? BracketName { get; set; }
        public string? SportName { get; set; }
        public List<SportsBracketRegions>? RegionsList { get; set; }
    }

    public class SportsBracketRegions
    {
        public int ID { get; set; }
        public int BracketID { get; set; }
        public int RegionID { get; set; }
        public string? RegionName { get; set; }
        public string? RegionAbbreviation { get; set; }
    }
}
