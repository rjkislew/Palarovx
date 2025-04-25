using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class EventStreamServices
{
    public int ID { get; set; }

    public string? StreamService { get; set; }

    [JsonIgnore]
    public virtual ICollection<EventStreams> EventStreams { get; set; } = new List<EventStreams>();
}
