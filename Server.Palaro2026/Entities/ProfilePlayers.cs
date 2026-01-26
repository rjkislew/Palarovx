using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class ProfilePlayers
{
    public string ID { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
    public string? Sex { get; set; }
    public int? SchoolID { get; set; }

    public int? SportID { get; set; }
    public string? MiddleInitial { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? LRN { get; set; }
    
    public int? SportCategoryID { get; set; }
    public string? UploadedBy { get; set; }
    public string? ImagePath { get; set; }

    [JsonIgnore]
    public virtual ICollection<EventVersusTeamPlayers> EventVersusTeamPlayers { get; set; } = new List<EventVersusTeamPlayers>();

    [JsonIgnore]
    public virtual ICollection<ProfilePlayerSports> ProfilePlayerSports { get; set; } = new List<ProfilePlayerSports>();

    public virtual Schools? School { get; set; }

    public virtual Sports? Sport { get; set; }
    public virtual SportCategories? SportCategory { get; set; }
    public virtual Users? UserID { get; set; }
}
