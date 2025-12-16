namespace Server.Palaro2026.DTO;

public class GeneralScheduleDTO
{
    public int ID { get; set; }
    public int? SportsID { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public List<ActivityDTO>? Activities { get; set; }
}

public class ActivityDTO
{
    public int ID { get; set; }
    public int ScheduleID { get; set; }
    public TimeSpan? Time { get; set; }
    public string? ActivityName { get; set; }
}