using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class Schools
{
    public int ID { get; set; }

    public string? School { get; set; }
    
    public int? SchoolRegionID { get; set; }

    public int? SchoolDivisionID { get; set; }

    public int? SchoolLevelsID { get; set; }
    public string? SchoolType { get; set; }
    public string? SchoolCode { get; set; }
    public string? SchoolAddress { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProfilePlayers> ProfilePlayers { get; set; } = new List<ProfilePlayers>();
    
    [JsonIgnore]
    public virtual ICollection<ProfileCoaches> ProfileCoaches { get; set; } = new List<ProfileCoaches>();

    public virtual SchoolDivisions? SchoolDivision { get; set; }

    public virtual SchoolLevels? SchoolLevels { get; set; }
    
    public virtual SchoolRegions? SchoolRegion { get; set; }
}
