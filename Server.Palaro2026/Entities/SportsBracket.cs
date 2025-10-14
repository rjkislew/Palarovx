using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class SportsBracket
{
    public int ID { get; set; }

    public int SportID { get; set; }

    public string? BracketName { get; set; }

    public virtual Sports? Sport { get; set; }

    [JsonIgnore]
    public virtual ICollection<SportsBracketRegions> SportsBracketRegions { get; set; } = new List<SportsBracketRegions>();
}
