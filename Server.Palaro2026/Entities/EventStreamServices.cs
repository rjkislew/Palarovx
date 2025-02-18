using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class EventStreamServices
{
    public int ID { get; set; }

    public string? StreamService { get; set; }

    public virtual ICollection<EventStreams> EventStreams { get; set; } = new List<EventStreams>();
}
