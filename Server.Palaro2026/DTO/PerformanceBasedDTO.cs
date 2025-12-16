namespace Server.Palaro2026.DTO;

public class PerformanceBasedDTO
{
    public class PerformanceEvent
    {
        public int ID { get; set; }
        public int? SportID { get; set; }
        public string MainCategory { get; set; }
        public int LevelID { get; set; }
        public int GenderID { get; set; }
        public int StageID { get; set; }
        public bool IsFinished { get; set; }
    }

    public class PerformanceTeam
    {
        public int ID { get; set; }
        public int PerformanceID { get; set; }
        public int? TeamID { get; set; }
        public int RegionID { get; set; }
        public string PlayerID { get; set; }
    }
    
    public class PerformanceScore
    {
        public int ID { get; set; }
        public int PerformanceID { get; set; }
        public int PerformanceTeamID { get; set; }
        public int SportSubcategoryID { get; set; }
        public decimal Score { get; set; }
        public int Rank { get; set; }
        public string? UserID { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? TeamID { get; set; }
    }
}