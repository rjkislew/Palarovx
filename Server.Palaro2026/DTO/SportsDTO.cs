namespace Server.Palaro2026.DTO;

public class SportsDTO
{
    public class SportDetails
    {
        public class SD_SportCategoriesContent
        {
            public string? Category { get; set; }
            public List<SD_SportsContent>? SportList { get; set; }
        }
        public class SD_SportsContent
        {
            public string? Sport { get; set; }
            public string? Description { get; set; }
            public List<SD_SchoolLevelsContent>? LevelList { get; set; }
        }
        public class SD_SchoolLevelsContent
        {
            public string? Level { get; set; }
            public List<SD_GenderCategoriesContent>? GenderList { get; set; }
        }
        public class SD_GenderCategoriesContent
        {
            public string? Gender { get; set; }
            public List<SD_SubCategoriesContent>? SubCategoryList { get; set; }
        }
        public class SD_SubCategoriesContent
        {
            public string? SubCategory { get; set; }
        }
    }

    public class SportCategoryDetails
    {
        public class SCD_CategoriesContent
        {
            public string? Category { get; set; }
            public List<SCD_SportsContent>? SportList { get; set; }
        }
        public class SCD_SportsContent
        {
            public string? Sport { get; set; }
            public string? Description { get; set; }
        }
    }

    public class SportCategoryAndSubCategoryDetails
    {
        public class SCASD_CategoriesContent
        {
            public string? Category { get; set; }
            public List<SCASD_SportsContent>? SportList { get; set; }
        }
        public class SCASD_SportsContent
        {
            public string? Sport { get; set; }
            public string? Description { get; set; }
            public List<SCASD_SubCategoriesContent>? SportSubCategoryList { get; set; }
        }
        public class SCASD_SubCategoriesContent
        {
            public string? SubCategory { get; set; }
        }
    }

    public class SportCategories
    {
        public class SportCategoriesContent
        {
            public int ID { get; set; }
            public string? Category { get; set; }

        }
    }

    public class Sports
    {
        public class SportsContent
        {
            public int ID { get; set; }
            public string? Sport { get; set; }
            public string? Description { get; set; }
            public int? SportCategoryID { get; set; }
        }
    }

    public class SchoolLevels
    {
        public class SchoolLevelsContent
        {
            public int ID { get; set; }
            public string? Level { get; set; }
        }
    }

    public class GenderCategories
    {
        public class GenderCategoriesContent
        {
            public int ID { get; set; }
            public string? Gender { get; set; }
        }

    }

    public class SportSubCategories
    {
        public class SportSubCategoriesContent
        {
            public int ID { get; set; }
            public string? SubCategory { get; set; }
            public int? SportID { get; set; }
            public int? GenderCategoryID { get; set; }
            public int? SchoolLevelID { get; set; }
        }
    }

}