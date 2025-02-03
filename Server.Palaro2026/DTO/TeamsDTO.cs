using Server.Palaro2026.Entities;

namespace Server.Palaro2026.DTO;

public class TeamsDTO
{
    public class TeamCoachProfiles
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? SchoolRegionID { get; set; }
        public int? SportSubcategoryID { get; set; }
    }

    public class TeamPlayerProfiles
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? SchoolID { get; set; }
        public int? SportSubcategoryID { get; set; }
    }
}
