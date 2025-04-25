using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class EventStages
{
    public int ID { get; set; }

    public string? Stage { get; set; }

    [JsonIgnore]
    public virtual ICollection<Events> Events { get; set; } = new List<Events>();
}
