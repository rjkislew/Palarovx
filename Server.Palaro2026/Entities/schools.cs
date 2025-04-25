using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class Schools
{
    public int ID { get; set; }

    public string? School { get; set; }

    public int? SchoolDivisionID { get; set; }

    public int? SchoolLevelsID { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProfilePlayers> ProfilePlayers { get; set; } = new List<ProfilePlayers>();

    public virtual SchoolDivisions? SchoolDivision { get; set; }

    public virtual SchoolLevels? SchoolLevels { get; set; }
}
