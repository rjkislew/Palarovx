using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class SchoolDivisions
{
    public int ID { get; set; }

    public string? Division { get; set; }

    public int? SchoolRegionID { get; set; }

    public virtual SchoolRegions? SchoolRegion { get; set; }

    [JsonIgnore]
    public virtual ICollection<Schools> Schools { get; set; } = new List<Schools>();
}
