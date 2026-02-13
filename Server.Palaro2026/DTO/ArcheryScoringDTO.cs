namespace Server.Palaro2026.DTO
{
    public class ArcheryEventDTO
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
        public int EndNo { get; set; } = 1;
        public string? Position { get; set; }
    }

    public class ArcheryScoringDTO
    {
        public int ID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RoundNo { get; set; }
        public int ShotNo { get; set; }
        public int ShotScore { get; set; }
        public bool IsBullseye { get; set; }
        public bool IsWinner { get; set; }
        public int RegionID { get; set; }
        public string? PlayerID { get; set; }
        public string? PlayerName { get; set; }
        public int? Lane { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int EndNo { get; set; } = 1;
        public string? Position { get; set; }
        public string? Region { get; set; }
        public string? Abbreviation { get; set; }
        public string? Sport { get; set; }
        public string? Subcategory { get; set; }
        public string? Gender { get; set; }
        public string? Level { get; set; }
        public string? EventStage { get; set; }
    }

    public class CreateArcheryScoringDTO
    {
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RoundNo { get; set; }
        public int ShotNo { get; set; }
        public int ShotScore { get; set; }
        public bool IsBullseye { get; set; }
        public bool IsWinner { get; set; }
        public int RegionID { get; set; }
        public string? PlayerID { get; set; }
        public int Lane { get; set; }
        public int EndNo { get; set; } = 1;
        public string? Position { get; set; }
    }

    public class UpdateArcheryScoringDTO
    {
        public int ID { get; set; }
        public int? RoundNo { get; set; }
        public int? ShotNo { get; set; }
        public int? ShotScore { get; set; }
        public bool? IsBullseye { get; set; }
        public bool? IsWinner { get; set; }
        public int? RegionID { get; set; }
        public string? PlayerID { get; set; }
        public int? Lane { get; set; }
        public int? EndNo { get; set; }
        public string? Position { get; set; }
    }
}