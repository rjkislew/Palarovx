using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class ProfileCoaches
{
    public string? ID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleInitial { get; set; }
    public string? Sex { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Designation { get; set; }
    public int? SportID { get; set; }
    public int? GenderCategoryID { get; set; }
    public int? SchoolRegionID { get; set; }
    public int? SchoolDivisionID { get; set; }
    public int? SchoolID { get; set; }
    public int? SportCategoryID { get; set; }
    public string? UploadedBy { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProfilePlayerSportCoaches> ProfilePlayerSportCoaches { get; set; } =
        new List<ProfilePlayerSportCoaches>();

    public virtual SchoolRegions? SchoolRegion { get; set; }
    public virtual Sports? Sport { get; set; }
    public virtual SportGenderCategories? SportGenderCategories { get; set; }
    public virtual SchoolDivisions? SchoolDivision { get; set; }
    public virtual Schools? School { get; set; }
    public virtual SportCategories? SportCategory { get; set; }
    public virtual Users? UserID { get; set; }
}



