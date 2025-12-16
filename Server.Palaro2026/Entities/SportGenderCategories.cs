using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class SportGenderCategories
{
    public int ID { get; set; }

    public string? Gender { get; set; }

    [JsonIgnore]
    public virtual ICollection<SportSubcategories> SportSubcategories { get; set; } = new List<SportSubcategories>();
    
    [JsonIgnore]
    public virtual ICollection<ProfileCoaches> ProfileCoaches { get; set; } = new List<ProfileCoaches>();
}
