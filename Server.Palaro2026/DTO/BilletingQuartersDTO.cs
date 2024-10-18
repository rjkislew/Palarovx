namespace Server.Palaro2026.DTO;

public class BilletingQuartersDTO
{
    public class bq_RegionBilletingQuartersDTO
    {
        public string? RegionalTeamName { get; set; }

        public string? RegionalTeamNameAbbreviation { get; set; }

        public string? SchoolName { get; set; }

        public string? SchoolAddress { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? ContactPerson { get; set; }

        public string? ContactPersonNumber { get; set; }
    }

    public class bq_BilletingQuartersDTO
    {
        public int ID { get; set; }

        public int? RegionalTeamID { get; set; }

        public string? SchoolName { get; set; }

        public string? SchoolAddress { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? ContactPerson { get; set; }

        public string? ContactPersonNumber { get; set; }
    }
}
