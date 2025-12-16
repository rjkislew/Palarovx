namespace Server.Palaro2026.DTO
{
    public class TeamMatchDTO
    {
        public int ID { get; set; }
        public string? EventID { get; set; }
        public int EventVersusID { get; set; }
        public int? RegionID { get; set; }
        public int Phase { get; set; }
        public int Score { get; set; }
        public bool IsWinner { get; set; }
        public bool IsSetWinner { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string? EventName { get; set; }
        public string? Region { get; set; }
        public string? RegionAbbreviation { get; set; }
        public string? Sport { get; set; }
        public string? EventStage { get; set; }
    }

    public class CreateTeamMatchDTO
    {
        public string? EventID { get; set; }
        public int EventVersusID { get; set; }
        public int? RegionID { get; set; }
        public int Phase { get; set; }
        public int Score { get; set; }
        public bool IsWinner { get; set; }
        public bool IsSetWinner { get; set; }
    }

    public class UpdateTeamMatchDTO
    {
        public int ID { get; set; }
        public int? Score { get; set; }
        public bool? IsWinner { get; set; }
        public int? RegionID { get; set; }
        public bool? IsSetWinner { get; set; }
    }

    public class TeamEventDTO
    {
        public string? ID { get; set; }
        public string? Sport { get; set; }
        public string? Subcategory { get; set; }
        public string? Gender { get; set; }
        public string? Level { get; set; }
        public string? EventStage { get; set; }
        public string? DisplayName => $"{Subcategory} - {Gender} - {Level}";
    }

    public class MarkSetWinnerDTO
    {
        public string EventID { get; set; } = "";
        public int Phase { get; set; }
        public int WinningRegionID { get; set; }
    }

    public class SetScoreInfo
    {
        public int SetNumber { get; set; }
        public int SetsWonByRegionA { get; set; }
        public int TotalSetsCompleted { get; set; }
    }

    public class SetScoresDTO
    {
        public string EventID { get; set; } = "";
        public List<SetScoreInfo> SetScores { get; set; } = new();
        public int TotalSets { get; set; }
        public int CompletedSets { get; set; }
    }
}