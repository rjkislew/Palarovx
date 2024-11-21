namespace Server.Palaro2026.DTO
{
    public class StreamServicesDTO
    {
        public class SteamDetails
        {
            public class SD_StreamServicesContent
            {
                public int StreamServiceID { get; set; }
                public string? StreamService { get; set; }
                public List<SD_StreamURLsContent>? StreamURLList { get; set; }
            }
            public class SD_StreamURLsContent
            {
                public int StreamURLID { get; set; }
                public DateTime? Date { get; set; }
                public string? StreamURL { get; set; }
                public bool? IsFinished { get; set; }
            }
        }

        public class StreamServices
        {
            public class StreamServiceContent
            {
                public int ID { get; set; }
                public string? StreamService { get; set; }
            }
        }

        public class StreamURLs
        {
            public class StreamURLContent
            {
                public int ID { get; set; }

                public int StreamServiceID { get; set; }

                public DateTime? Date { get; set; }

                public string? StreamURL { get; set; }

                public bool? IsFinished { get; set; }
            }
        }
    }
}
