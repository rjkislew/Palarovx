using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class SchoolLevels
{
    public int ID { get; set; }

    public string? Level { get; set; }

    [JsonIgnore]
    public virtual ICollection<Schools> Schools { get; set; } = new List<Schools>();

    [JsonIgnore]
    public virtual ICollection<SportSubcategories> SportSubcategories { get; set; } = new List<SportSubcategories>();
}
