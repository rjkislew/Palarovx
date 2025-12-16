using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class SportCategories
{
    public int ID { get; set; }

    public string? Category { get; set; }

    [JsonIgnore]
    public virtual ICollection<Sports> Sports { get; set; } = new List<Sports>();
    
    [JsonIgnore]
    public virtual ICollection<ProfilePlayers> ProfilePlayers { get; set; } = new List<ProfilePlayers>();
    
    [JsonIgnore]
    public virtual ICollection<ProfileCoaches> ProfileCoaches { get; set; } = new List<ProfileCoaches>();
}
