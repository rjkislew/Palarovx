using Server.Palaro2026.Entities;

namespace Server.Palaro2026.DTO;

public class ProfilesDTO
{
    public class ProfileCoachesDetails
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Region { get; set; }
        public string? Abbreviation { get; set; }
    }

    public class ProfileCoaches
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? SchoolRegionID { get; set; }
    }

    public class ProfilePlayersDetails
    {
        public class ProfilePlayers
        {
            public int ID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? School { get; set; }
            public int? SchoolLevelID { get; set; }
            public string? Level { get; set; }
            public string? Division { get; set; }
            public string? Region { get; set; }
            public string? Abbreviation { get; set; }
            public string? Category { get; set; }
            public int? SportID { get; set; }
            public string? Sport { get; set; }
            public List<ProfilePlayerSports>? ProfilePlayerSportsList { get; set; }
        }

        public class ProfilePlayerSports
        {
            public string? Subcategory { get; set; }
            public string? Gender { get; set; }
            public List<ProfilePlayerSportCoaches>? ProfilePlayerSportCoachesList { get; set; }
        }

        public class ProfilePlayerSportCoaches
        {
            public string? CoachFirstName { get; set; }
            public string? CoachLastName { get; set; }
        }
    }

    public class ProfilePlayers
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? SchoolID { get; set; }
        public int? SportID { get; set; }
    }

    public class ProfilePlayerSports
    {
        public int ID { get; set; }
        public int ProfilePlayerID { get; set; }
        public int? ProfilePlayerSportCoachID { get; set; }
        public int? SportSubcategoryID { get; set; }
    }

    public class ProfilePlayerSportCoaches
    {
        public int ID { get; set; }
        public int? ProfileCoachID { get; set; }
        public int? ProfilePlayerSportID { get; set; }
    }
}
