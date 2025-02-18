using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class SchoolLevels
{
    public int ID { get; set; }

    public string? Level { get; set; }

    public virtual ICollection<Schools> Schools { get; set; } = new List<Schools>();

    public virtual ICollection<SportSubcategories> SportSubcategories { get; set; } = new List<SportSubcategories>();
}
