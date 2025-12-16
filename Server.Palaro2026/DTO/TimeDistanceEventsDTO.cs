namespace Server.Palaro2026.DTO
{
    public class TimeDistanceEventsDTO
    {
        public class TimeDistanceEvent
        {
            public int ID { get; set; }
            public string EventID { get; set; } = null!;
            public int RegionID { get; set; }
            public string? PlayerID { get; set; }
            public int HeatNo { get; set; }
            public int LaneNo { get; set; }
            public string? Result { get; set; }
            public int? Rank { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class SaveAthleticsSwimmingRequest
        {
            public string EventID { get; set; } = null!;
            public List<HeatRequest> Heats { get; set; } = new();
            public List<AssignmentRequest> Assignments { get; set; } = new();
        }

        public class HeatRequest
        {
            public string HeatName { get; set; } = null!;
            public int HeatOrder { get; set; }
            public List<LaneRequest> Lanes { get; set; } = new();
        }

        public class LaneRequest
        {
            public string LaneName { get; set; } = null!;
            public int LaneOrder { get; set; }
            public string? Result { get; set; }
            public int? RegionID { get; set; }
            public string? PlayerID { get; set; }
            public string? PlayerName { get; set; }
            public string? RegionAbbreviation { get; set; }
        }

        public class AssignmentRequest
        {
            public int RegionID { get; set; }
            public string? PlayerID { get; set; }
            public string RegionAbbreviation { get; set; } = null!;
            public string AssignedTable { get; set; } = null!;
        }
    }
}