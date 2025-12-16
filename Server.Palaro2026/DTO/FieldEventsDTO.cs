namespace Server.Palaro2026.DTO
{
    public class FieldEventsDTO
    {
        public class SaveFieldEventsRequest
        {
            public string EventID { get; set; } = null!;
            public List<SaveFieldEventData> Results { get; set; } = new();
        }

        public class SaveFieldEventData
        {
            public string EventID { get; set; } = null!;
            public int RegionID { get; set; }
            public string? PlayerID { get; set; }
            public int AttemptNo { get; set; }
            public decimal Result { get; set; }
            public int? Rank { get; set; }
        }

        public class FieldEventResult
        {
            public string EventID { get; set; } = null!;
            public int RegionID { get; set; }
            public string? PlayerID { get; set; }
            public int AttemptNo { get; set; }
            public decimal Result { get; set; }
            public int? Rank { get; set; }
        }

        public class FieldEventRanking
        {
            public int RegionID { get; set; }
            public string? PlayerID { get; set; }
            public decimal BestResult { get; set; }
            public string RegionAbbreviation { get; set; } = null!;
            public string? PlayerName { get; set; }
            public int Rank { get; set; }
        }
    }
}