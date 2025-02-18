using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class EventStreams
{
    public int ID { get; set; }

    public int? EventStreamServiceID { get; set; }

    public string? StreamTitle { get; set; }

    public string? StreamURL { get; set; }

    public DateTime? StreamDate { get; set; }

    public virtual EventStreamServices? EventStreamService { get; set; }

    public virtual ICollection<Events> Events { get; set; } = new List<Events>();
}
