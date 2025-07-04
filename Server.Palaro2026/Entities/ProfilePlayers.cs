using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class ProfilePlayers
{
    public string ID { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? SchoolID { get; set; }

    public int? SportID { get; set; }

    [JsonIgnore]
    public virtual ICollection<EventVersusTeamPlayers> EventVersusTeamPlayers { get; set; } = new List<EventVersusTeamPlayers>();

    [JsonIgnore]
    public virtual ICollection<ProfilePlayerSports> ProfilePlayerSports { get; set; } = new List<ProfilePlayerSports>();

    public virtual Schools? School { get; set; }

    public virtual Sports? Sport { get; set; }
}
