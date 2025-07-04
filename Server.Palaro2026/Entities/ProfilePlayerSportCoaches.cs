using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class ProfilePlayerSportCoaches
{
    public int ID { get; set; }

    public int? ProfilePlayerSportID { get; set; }

    public string? ProfileCoachID { get; set; }

    public virtual ProfileCoaches? ProfileCoach { get; set; }

    public virtual ProfilePlayerSports? ProfilePlayerSport { get; set; }
}
