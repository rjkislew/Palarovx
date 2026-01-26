namespace Server.Palaro2026.DTO
{
    public class BilliardsScoringDTO
    {
        public int ID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int SetNo { get; set; }
        public int TableNo { get; set; }
        public string? PlayerPosition { get; set; }
        public int? Score { get; set; }
        public bool? IsWinner { get; set; }
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

    public class BilliardsEventDTO
    {
        public string? ID { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? Sport { get; set; }
        public string? Subcategory { get; set; }
        public string? Gender { get; set; }
        public string? Level { get; set; }
        public string? EventStage { get; set; }
        public string? SportMainCat { get; set; }
        public string? Region { get; set; }
        public string? Abbreviation { get; set; }
        public int RegionID { get; set; }
        public string? PlayerID { get; set; }
        public string? PlayerName { get; set; }
    }

    public class CreateBilliardsScoringDTO
    {
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int SetNo { get; set; }
        public int TableNo { get; set; }
        public string? PlayerPosition { get; set; }
        public int? Score { get; set; }
        public bool? IsWinner { get; set; }
        public string? PlayerID { get; set; }
    }

    public class UpdateBilliardsScoringDTO
    {
        public int ID { get; set; }
        public int? TableNo { get; set; }
        public string? PlayerPosition { get; set; }
        public int? Score { get; set; }
        public bool? IsWinner { get; set; }
        public int? SetNo { get; set; }
        public string? PlayerID { get; set; }
    }
}