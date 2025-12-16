namespace Server.Palaro2026.DTO;

public class SchoolsDTO
{
    public class SchoolDetails
    {
        public class Schools
        {
            public int? ID { get; set; }
            public string? School { get; set; }
            public string? Level { get; set; }
            public string? Division { get; set; }
            public string? Region { get; set; }
            public string? Abbreviation { get; set; }
            public string? SchoolType { get; set; }
            public string? SchoolCode { get; set; }
            public string? SchoolAddress { get; set; }
        }
    }

    public class SchoolBillingQuarterDetails
    {
        public class SchoolBilletingQuarters
        {
            public int ID { get; set; }
            public string? Region { get; set; }
            public string? Abbreviation { get; set; }
            public string? BilletingQuarter { get; set; }
            public string? Address { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string? ContactPerson { get; set; }
            public string? ContactPersonNumber { get; set; }
        }
    }

    public class Schools
    {
        public int ID { get; set; }
        public string? School { get; set; }
        public int? SchoolRegionID { get; set; }
        public int? SchoolDivisionID { get; set; }
        public int? SchoolLevelsID { get; set; }
        public string? SchoolType { get; set; }
        public string? SchoolCode { get; set; }
        public string? SchoolAddress { get; set; }
    }

    public class SchoolBilletingQuarters
    {
        public int ID { get; set; }
        public int? SchoolRegionID { get; set; }
        public string? BilletingQuarter { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPersonNumber { get; set; }
    }

    public class SchoolRegions
    {
        public int ID { get; set; }
        public string? Region { get; set; }
        public string? Abbreviation { get; set; }
    }

    public class SchoolDivisions
    {
        public int ID { get; set; }
        public string? Division { get; set; }
        public int? SchoolRegionID { get; set; }
    }

    public class SchoolDivisionsDetails
    {
        public int ID { get; set; }
        public string? Division { get; set; }
        public int? SchoolRegionID { get; set; }
        public string? Region { get; set; }
        public string? Abbreviation { get; set; }
    }

    public class SchoolLevels
    {
        public int ID { get; set; }
        public string? Level { get; set; }
    }
}
