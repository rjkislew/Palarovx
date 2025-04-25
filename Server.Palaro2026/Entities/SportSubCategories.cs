using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class SportSubcategories
{
    public int ID { get; set; }

    public string? Subcategory { get; set; }

    public int? SportID { get; set; }

    public int? SportGenderCategoryID { get; set; }

    public int? SchoolLevelID { get; set; }

    [JsonIgnore]
    public virtual ICollection<Events> Events { get; set; } = new List<Events>();

    [JsonIgnore]
    public virtual ICollection<ProfilePlayerSports> ProfilePlayerSports { get; set; } = new List<ProfilePlayerSports>();

    public virtual SchoolLevels? SchoolLevel { get; set; }

    public virtual Sports? Sport { get; set; }

    public virtual SportGenderCategories? SportGenderCategory { get; set; }
}
