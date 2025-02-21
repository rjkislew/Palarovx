using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class EventVersusTeams
{
    public int ID { get; set; }

    public string? EventID { get; set; }

    public int? SchoolRegionID { get; set; }

    public string? Score { get; set; }

    public DateTime? RecentUpdateAt { get; set; }

    public virtual Events? Event { get; set; }

    public virtual ICollection<EventVersusTeamPlayers> EventVersusTeamPlayers { get; set; } = new List<EventVersusTeamPlayers>();

    public virtual SchoolRegions? SchoolRegion { get; set; }
}
