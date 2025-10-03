using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class SchoolRegions
{
    public int ID { get; set; }

    public string? Region { get; set; }

    public string? Abbreviation { get; set; }

    [JsonIgnore]
    public virtual ICollection<EventVersusTeams> EventVersusTeams { get; set; } = new List<EventVersusTeams>();
    //
    [JsonIgnore]
    public virtual ICollection<EventVersusTeamScores> EventVersusTeamScores { get; set; } = new List<EventVersusTeamScores>();
    //

    [JsonIgnore]
    public virtual ICollection<ProfileCoaches> ProfileCoaches { get; set; } = new List<ProfileCoaches>();

    [JsonIgnore]
    public virtual ICollection<SchoolBilletingQuarters> SchoolBilletingQuarters { get; set; } = new List<SchoolBilletingQuarters>();

    [JsonIgnore]
    public virtual ICollection<SchoolDivisions> SchoolDivisions { get; set; } = new List<SchoolDivisions>();
}
