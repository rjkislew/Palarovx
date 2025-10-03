using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public class EventVersusTeamScores
{
    public int ID { get; set; }

    public string? EventID { get; set; }

    public int? SchoolRegionID { get; set; }

    public int PhaseNumber { get; set; }
    public string? Point { get; set; }

    public string? Rank { get; set; }

    public DateTime? RecentUpdateAt { get; set; }

    public virtual Events? Event { get; set; }

    [JsonIgnore]
    public virtual ICollection<EventVersusTeamPlayers> EventVersusTeamPlayers { get; set; } = new List<EventVersusTeamPlayers>();

    public virtual SchoolRegions? SchoolRegion { get; set; }
}





/////new table for per phase scoring

