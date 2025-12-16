namespace Server.Palaro2026.DTO
{
    public class DragndropDTO
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
        public string? EventName { get; set; }
    }

    public class CreateDragndropDTO
    {
        public int? SportsID { get; set; }
        public string? EventID { get; set; }
        public int? EventVersusID { get; set; }
        public int RegionID { get; set; }
        public int SetNo { get; set; }
        public int TableNo { get; set; }
        public string? ColorGroup { get; set; }
        public decimal? Score { get; set; }
    }

    public class UpdateDragndropDTO
    {
        public int ID { get; set; }
        public int? TableNo { get; set; }
        public string? ColorGroup { get; set; }
        public decimal? Score { get; set; }
        public int? SetNo { get; set; }
    }
}
