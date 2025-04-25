using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class Sports
{
    public int ID { get; set; }

    public string? Sport { get; set; }

    public string? Description { get; set; }

    public int? SportCategoryID { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProfilePlayers> ProfilePlayers { get; set; } = new List<ProfilePlayers>();

    public virtual SportCategories? SportCategory { get; set; }

    [JsonIgnore]
    public virtual ICollection<SportSubcategories> SportSubcategories { get; set; } = new List<SportSubcategories>();
}
