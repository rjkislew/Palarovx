namespace Server.Palaro2026.DTO;

public class BilletingQuartersDTO
{
    public class BilletingQuartersDetails
    {
        public class BQD_RegionContent
        {
            public string? Region { get; set; }
            public string? Abbreviation { get; set; }
            public List<BQD_BilletingQuartersContent>? BilletingQuarterList { get; set; }
        }

        public class BQD_BilletingQuartersContent
        {
            public string? BilletingQuarter { get; set; }
            public string? Address { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string? ContactPerson { get; set; }
            public string? ContactPersonNumber { get; set; }

        }
    }
    public class BilletingQuarters
    {
        public class BilletingQuartersContent
        {
            public int ID { get; set; }
            public int? RegionID { get; set; }
            public string? BilletingQuarter { get; set; }
            public string? Address { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string? ContactPerson { get; set; }
            public string? ContactPersonNumber { get; set; }
        }
    }
}