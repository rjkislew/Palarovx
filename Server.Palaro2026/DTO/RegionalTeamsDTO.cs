namespace Server.Palaro2026.DTO
{
    public class RegionalTeamsDTO
    {
        public class SchoolDetails
        {
            public partial class SD_RegionsContent
            {
                public string? Region { get; set; }
                public string? Abbreviation { get; set; }
                public List<SD_DivisionsContent>? DivisionList { get; set; }
            }
            public partial class SD_DivisionsContent
            {
                public string? Division { get; set; }
                public List<SD_SchoolsContent>? SchoolList { get; set; }
            }
            public partial class SD_SchoolsContent
            {
                public string? School { get; set; }
            }
        }

        public class RegionsDetail
        {
            public partial class RegionsContent
            {
                public int ID { get; set; }
                public string? Region { get; set; }
                public string? Abbreviation { get; set; }
            }
        }

        public class DivisionsDetail
        {
            public class DivisionsContent
            {
                public int ID { get; set; }
                public string? Division { get; set; }
                public int? RegionID { get; set; }
            }
        }

        public class SchoolsDetail
        {
            public class SchoolsContent
            {
                public int ID { get; set; }
                public string? School { get; set; }
            }
        }
    }
}
