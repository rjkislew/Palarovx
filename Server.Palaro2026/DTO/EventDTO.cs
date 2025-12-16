namespace Server.Palaro2026.DTO;

public class EventDTO
{
    public class EventDetails
    {
        public string? ID { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? Sport { get; set; }
        public string? Subcategory { get; set; }
        public string? Gender { get; set; }
        public string? Level { get; set; }
        public string? EventStage { get; set; }
        public bool? OnStream { get; set; }
        public bool? IsFinished { get; set; }
        public string? SportMainCat { get; set; }
        public bool? Archived { get; set; }
        public bool? Deleted { get; set; }
        public string? Region { get; set; }
        public string? Abbreviation { get; set; }
        public int RegionID { get; set; }
        public string? PlayerID { get; set; }
        public string? PlayerName { get; set; }
    }
}