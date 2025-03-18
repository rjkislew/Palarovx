namespace Server.Palaro2026.DTO;

public class EventsDTO
{
    public class EventDetails
    {
        public class Event
        {
            public string ID { get; set; } = null!;
            public string? EventStage { get; set; }
            public List<EventVersusTeams>? EventVersusList { get; set; }
            public string? Category { get; set; }
            public string? Sport { get; set; }
            public int? SubCategoryID { get; set; }
            public string? Subcategory { get; set; }
            public string? Gender { get; set; }
            public string? Level { get; set; }
            public string? Venue { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public DateTime? Date { get; set; }
            public TimeSpan? Time { get; set; }
            public bool? OnStream { get; set; }
            public string? StreamService { get; set; }
            public string? StreamURL { get; set; }
            public string? StreamTitle { get; set; }
            public bool? IsFinished { get; set; }
            public byte[]? Attachement { get; set; }
            public bool? Archived { get; set; }
            public bool? Deleted { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
        }

        public class EventVersusTeams
        {
            public int ID { get; set; }
            public string? Score { get; set; }
            public string? Region { get; set; }
            public string? Abbreviation { get; set; }
            public List<EventVersusTeamPlayers>? EventVersusTeamPlayersList { get; set; }
            public DateTime? RecentUpdateAt { get; set; }
        }

        public class EventVersusTeamPlayers
        {
            public int ID { get; set; }
            public int? EventVersusID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? School { get; set; }
        }
    }

    public class EventTally
    {
        public class RegionalTally
        {
            public string ID { get; set; } = null!;
            public string? EventStage { get; set; }
            public List<EventVersusTeams>? EventVersusList { get; set; }
            public string? Category { get; set; }
            public string? Sport { get; set; }
            public string? Subcategory { get; set; }
            public string? Gender { get; set; }
            public string? Level { get; set; }
        }

        public class EventVersusTeams
        {
            public int ID { get; set; }
            public string? Score { get; set; }
            public List<EventVersusTeamPlayers>? EventVersusTeamPlayersList { get; set; }
        }

        public class EventVersusTeamPlayers
        {
            public int ID { get; set; }
            public int? EventVersusID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? School { get; set; }
        }
    }

    public class Events
    {
        public string ID { get; set; } = null!;
        public int? EventStageID { get; set; }
        public int? SportSubcategoryID { get; set; }
        public int? EventVenuesID { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public bool? OnStream { get; set; }
        public int? EventStreamID { get; set; }
        public bool? IsFinished { get; set; }
        public byte[]? Attachement { get; set; }
        public bool? Archived { get; set; }
        public bool? Deleted { get; set; }
        public string? UserID { get; set; }
    }

    public class EventNews
    {
        public int ID { get; set; }
        public string? FacebookLink { get; set; }
    }

    public class EventStages
    {
        public int ID { get; set; }
        public string? Stage { get; set; }
    }

    public class EventStreamServicesDetails
    {
        public class EventStreamServices
        {
            public int ID { get; set; }
            public string? StreamService { get; set; }
            public List<EventStreams>? EventStreamsList { get; set; }
        }

        public class EventStreams
        {
            public int StreamID { get; set; }
            public string? StreamTitle { get; set; }
            public string? StreamURL { get; set; }
            public DateTime? StreamDate { get; set; }

        }
    }

    public class EventStreamServices
    {
        public int ID { get; set; }
        public string? StreamService { get; set; }
    }

    public class EventStreams
    {
        public int ID { get; set; }
        public int? EventStreamServiceID { get; set; }
        public string? StreamTitle { get; set; }
        public string? StreamURL { get; set; }
        public DateTime? StreamDate { get; set; }

    }

    public class EventVenues
    {
        public int ID { get; set; }
        public string? Address { get; set; }
        public string? Venue { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class EventVersusTeams
    {
        public int ID { get; set; }
        public string? EventID { get; set; }
        public int? SchoolRegionID { get; set; }
        public string? Score { get; set; }
        public DateTime? RecentUpdateAt { get; set; }
    }

    public class EventVersusTeamPlayers
    {
        public int ID { get; set; }
        public int? EventVersusID { get; set; }
        public int? ProfilePlayerID { get; set; }
    }
}
