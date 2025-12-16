namespace Server.Palaro2026.DTO
{
    public class ChessScoringDTO
    {
        public int ID { get; set; }
        public int? SportsID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int SetNo { get; set; }
        public int TableNo { get; set; }
        public string? ColorGroup { get; set; }
        public decimal? Score { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Region { get; set; }
        public string? Abbreviation { get; set; }
        public string? Sport { get; set; }
        public string? Subcategory { get; set; }
        public string? Gender { get; set; }
        public string? Level { get; set; }
        public string? EventStage { get; set; }
        public string? PlayerID { get; set; }
        public string? PlayerName { get; set; }
    }

    public class ChessEventDTO
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

    public class CreateChessScoringDTO
    {
        public int? SportsID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int SetNo { get; set; }
        public int TableNo { get; set; }
        public string? ColorGroup { get; set; }
        public decimal? Score { get; set; }
        public string? PlayerID { get; set; }
    }

    public class UpdateChessScoringDTO
    {
        public int ID { get; set; }
        public int? TableNo { get; set; }
        public string? ColorGroup { get; set; }
        public decimal? Score { get; set; }
        public int? SetNo { get; set; }
        public string? PlayerID { get; set; }
    }
}