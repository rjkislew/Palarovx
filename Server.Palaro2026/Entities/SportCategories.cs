using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class SportCategories
{
    public int ID { get; set; }

    public string? Category { get; set; }

    public virtual ICollection<Sports> Sports { get; set; } = new List<Sports>();
}
