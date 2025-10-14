using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;
public partial class SportsBracketRegions
{
    public int ID { get; set; }

    public int BracketID { get; set; }

    public int RegionID { get; set; }

    public virtual SportsBracket? Bracket { get; set; }

    public virtual SchoolRegions? Region { get; set; }
}

