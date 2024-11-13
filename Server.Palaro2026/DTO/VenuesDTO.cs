namespace Server.Palaro2026.DTO
{
    public class VenuesDTO
    {
        public class Venues
        {
            public class VenuesContents
            {
                public int ID { get; set; }
                public string? Venue { get; set; }
                public decimal? Latitude { get; set; }
                public decimal? Longitude { get; set; }

            }

        }
    }
}
