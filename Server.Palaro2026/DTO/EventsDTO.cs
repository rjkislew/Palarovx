namespace Server.Palaro2026.DTO;

public class EventsDTO
{
    public class e_EventsDTO
    {
        public int ID { get; set; }
        public string? TeamA { get; set; }
        public string? TeamAAbbreviation { get; set; }
        public int? TeamAFinalScore { get; set; }
        public string? TeamB { get; set; }
        public string? TeamBAbbreviation { get; set; }
        public int? TeamBFinalScore { get; set; }
        public string? SportSubCategory { get; set; }
        public string? Venue { get; set; }
        public string? EventTitle { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public bool? OnStream { get; set; }
        public string? StreamURL { get; set; }
        public bool? IsFinished { get; set; }
        public bool? Archived { get; set; }
        public bool? Deleted { get; set; }
        public byte[]? Attachement { get; set; }
        public string? LoserTeam { get; set; }
        public string? LoserTeamAbbreviation { get; set; }
        public string? WinnerTeam { get; set; }
        public string? WinnerTeamAbbreviation { get; set; }
    }
    public class e_CreateEventDTO
    {
        public int? RegionalTeamAID { get; set; }
        public int? TeamAFinalScore { get; set; }
        public int? RegionalTeamBID { get; set; }
        public int? TeamBFinalScore { get; set; }
        public int? SportSubCategoryID { get; set; }
        public int? venueID { get; set; }
        public string? EventTitle { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public bool? OnStream { get; set; }
        public string? StreamURL { get; set; }
    }

}