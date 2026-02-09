namespace Server.Palaro2026.DTO
{
    public class TabulationResultDTO
    {
        public string SourceType { get; set; } = "";

        public string? EventID { get; set; }
        public int? EventStageID { get; set; }
        public int SportSubcategoryID { get; set; }
        public string? SportMainCat { get; set; }
        public bool IsFinished { get; set; }

        public int? PerformanceID { get; set; }
        public int? TeamID { get; set; }

        public int? SchoolLevelID { get; set; }
        public string? Level { get; set; }


        public int? EventVersusTeamID { get; set; }
        public int? SchoolRegionID { get; set; }
        public string? TeamRank { get; set; }
        public int? PerformanceScoreID { get; set; }

        public string PlayerID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleInitial { get; set; }
        public string? Sex { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? SchoolID { get; set; }
        public int? SportID { get; set; }
        public string? ImagePath { get; set; }

        // Query-specific columns
        public int RankGroup { get; set; }
        public int MedalOrder { get; set; }
        public int NumericOrder { get; set; }
        public int rn_global { get; set; }
    }
}
