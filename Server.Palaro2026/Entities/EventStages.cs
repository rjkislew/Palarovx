using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class EventStages
{
    public int ID { get; set; }

    public string? Stage { get; set; }

    public virtual ICollection<Events> Events { get; set; } = new List<Events>();
}
