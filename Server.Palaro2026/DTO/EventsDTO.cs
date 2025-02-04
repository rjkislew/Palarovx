using System.Net.Mail;

namespace Server.Palaro2026.DTO;

public class EventsDTO
{
    public class EventDetails
    {
        public class Event
        {
            public string ID { get; set; } = null!;
            public List<EventVersus>? EventVersusList { get; set; }

            //public int? SportSubcategoryID { get; set; }
                public string? Category { get; set; }
                public string? Sport { get; set; }
                public string? Subcategory { get; set; }
                public string? Gender { get; set; }
                public string? Level { get; set; }
            //

            // public int? EventVenuesID { get; set; }
                public string? Venue { get; set; }
                public decimal? Latitude { get; set; }
                public decimal? Longitude { get; set; }
            //

            public DateTime? Date { get; set; }
            public TimeSpan? Time { get; set; }
            public bool? OnStream { get; set; }


            // public int? StreamID { get; set; }
                public string? StreamService { get; set; }
                public string? StreamURL { get; set; }
            //

            public bool? IsFinished { get; set; }
            public byte[]? Attachement { get; set; }
            public bool? Archived { get; set; }
            public bool? Deleted { get; set; }

            //public string? UserID { get; set; }
                public string? FirstName { get; set; }
                public string? LastName { get; set; }
        }

        public class EventVersus
        {
            public int? Score { get; set; }
            // public int? SchoolRegionID { get; set; }
                public string? Region { get; set; }
                public string? Abbreviation { get; set; }
            //

            public DateTime? RecentUpdateAt { get; set; }
        }
    }

    public class Events
    {
        public string ID { get; set; } = null!;
        public int? SportSubcategoryID { get; set; }
        public int? EventVenuesID { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public bool? OnStream { get; set; }
        public int? StreamID { get; set; }
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

    public class EventStreams
    {
        public int ID { get; set; }
        public string? StreamService { get; set; }
        public string? StreamURL { get; set; }
    }

    public class EventVenues
    {
        public int ID { get; set; }
        public string? Venue { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class EventVersus
    {
        public int ID { get; set; }
        public int? Score { get; set; }
        public int? SchoolRegionID { get; set; }
        public string? EventID { get; set; }
        public DateTime? RecentUpdateAt { get; set; }
    }
}
