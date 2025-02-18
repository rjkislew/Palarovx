using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class ProfilePlayers
{
    public int ID { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? SchoolID { get; set; }

    public int? SportID { get; set; }

    public virtual ICollection<ProfilePlayerSports> ProfilePlayerSports { get; set; } = new List<ProfilePlayerSports>();

    public virtual Schools? School { get; set; }

    public virtual Sports? Sport { get; set; }
}
