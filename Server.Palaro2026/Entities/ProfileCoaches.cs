using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class ProfileCoaches
{
    public string ID { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? SchoolRegionID { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProfilePlayerSportCoaches> ProfilePlayerSportCoaches { get; set; } = new List<ProfilePlayerSportCoaches>();

    public virtual SchoolRegions? SchoolRegion { get; set; }
}
