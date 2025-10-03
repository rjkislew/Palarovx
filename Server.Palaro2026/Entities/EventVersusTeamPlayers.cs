using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class EventVersusTeamPlayers
{
    public int ID { get; set; }

    public int? EventVersusID { get; set; }
    //
    public int? EventVersusTeamScoreID { get; set; }
    //

    public string? ProfilePlayerID { get; set; }

    public virtual EventVersusTeams? EventVersus { get; set; }
    //
    public virtual EventVersusTeamScores? EventVersusTeamScores { get; set; }
    //

    public virtual ProfilePlayers? ProfilePlayer { get; set; }
}
