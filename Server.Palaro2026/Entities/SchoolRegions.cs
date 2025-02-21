using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class SchoolRegions
{
    public int ID { get; set; }

    public string? Region { get; set; }

    public string? Abbreviation { get; set; }

    public virtual ICollection<EventVersusTeams> EventVersusTeams { get; set; } = new List<EventVersusTeams>();

    public virtual ICollection<ProfileCoaches> ProfileCoaches { get; set; } = new List<ProfileCoaches>();

    public virtual ICollection<SchoolBilletingQuarters> SchoolBilletingQuarters { get; set; } = new List<SchoolBilletingQuarters>();

    public virtual ICollection<SchoolDivisions> SchoolDivisions { get; set; } = new List<SchoolDivisions>();
}
