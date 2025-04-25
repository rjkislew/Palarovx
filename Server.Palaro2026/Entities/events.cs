using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class Events
{

    public string ID { get; set; } = null!;

    public int? EventStageID { get; set; }

    public int? SportSubcategoryID { get; set; }

    public int? EventVenuesID { get; set; }

    public DateTime? Date { get; set; }

    public TimeSpan? Time { get; set; }

    public bool? OnStream { get; set; }

    public int? EventStreamID { get; set; }

    public bool? IsFinished { get; set; }

    public byte[]? Attachement { get; set; }

    public bool? Archived { get; set; }

    public bool? Deleted { get; set; }

    public string? UserID { get; set; }

    public virtual EventStages? EventStage { get; set; }

    public virtual EventStreams? EventStream { get; set; }

    public virtual EventVenues? EventVenues { get; set; }

    [JsonIgnore]
    public virtual ICollection<EventVersusTeams> EventVersusTeams { get; set; } = new List<EventVersusTeams>();

    public virtual SportSubcategories? SportSubcategory { get; set; }

    public virtual Users? User { get; set; }
}
