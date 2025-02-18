using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class Events
{
    public string ID { get; set; } = null!;

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

    public virtual EventStreams? EventStream { get; set; }

    public virtual EventVenues? EventVenues { get; set; }

    public virtual ICollection<EventVersus> EventVersus { get; set; } = new List<EventVersus>();

    public virtual SportSubcategories? SportSubcategory { get; set; }

    public virtual Users? User { get; set; }
}
