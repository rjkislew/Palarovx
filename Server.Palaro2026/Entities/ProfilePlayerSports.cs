using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class ProfilePlayerSports
{
    public int ID { get; set; }

    public int? ProfilePlayerID { get; set; }

    public int? SportSubcategoryID { get; set; }

    public virtual ProfilePlayers? ProfilePlayer { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProfilePlayerSportCoaches> ProfilePlayerSportCoaches { get; set; } = new List<ProfilePlayerSportCoaches>();

    public virtual SportSubcategories? SportSubcategory { get; set; }
}
