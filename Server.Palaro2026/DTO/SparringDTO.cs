namespace Server.Palaro2026.DTO
{
    public class SparringDTO
    {
        public int ID { get; set; }
        public int? SportsID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int RoundNo { get; set; }
        public int MatNo { get; set; }
        public string? Corner { get; set; }
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

    public class CreateSparringDTO
    {
        public int? SportsID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int RoundNo { get; set; }
        public int MatNo { get; set; }
        public string? Corner { get; set; }
        public string? Result { get; set; }
        public string? PlayerID { get; set; }
    }

    public class UpdateSparringDTO
    {
        public int ID { get; set; }
        public int? MatNo { get; set; }
        public string? Corner { get; set; }
        public string? Result { get; set; }
        public int? RoundNo { get; set; }
        public string? PlayerID { get; set; }
    }

    public class SparringEventDTO
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

    public class SparringSetDTO
    {
        public int SetNumber { get; set; }
        public int RedScore { get; set; }
        public int BlueScore { get; set; }
        public string? Winner { get; set; } 
        public bool IsCompleted { get; set; }
        public string MatName { get; set; } = "";
    }

    public class SparringMatchDTO
    {
        public string MatName { get; set; } = "";
        public List<SparringSetDTO> Sets { get; set; } = new();
        public string? MatchWinner { get; set; }
        public bool MatchCompleted { get; set; }
    }

    public class CreateSparringSetResultDTO
    {
        public string? EventID { get; set; }
        public string MatName { get; set; } = "";
        public int SetNumber { get; set; }
        public int RedScore { get; set; }
        public int BlueScore { get; set; }
        public string? Winner { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class CreateSparringMatchResultDTO
    {
        public string? EventID { get; set; }
        public string MatName { get; set; } = "";
        public string? MatchWinner { get; set; }
        public bool MatchCompleted { get; set; }
    }
}