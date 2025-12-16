namespace Server.Palaro2026.DTO
{
    public class MartialArtsScoringDTO
    {
        public int ID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int GameNo { get; set; }
        public int MatchId { get; set; }
        public string? MatchPosition { get; set; }
        public int SetNo { get; set; }
        public string? Result { get; set; }
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

    public class MartialArtsEventDTO
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

    public class CreateMartialArtsScoringDTO
    {
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int GameNo { get; set; }
        public int MatchId { get; set; }
        public string? MatchPosition { get; set; }
        public int SetNo { get; set; }
        public string? Result { get; set; }
        public string? PlayerID { get; set; }
    }

    public class UpdateMartialArtsScoringDTO
    {
        public int ID { get; set; }
        public int GameNo { get; set; }
        public int MatchId { get; set; }
        public string? MatchPosition { get; set; }
        public int SetNo { get; set; }
        public string? Result { get; set; }
        public string? PlayerID { get; set; }
    }
}