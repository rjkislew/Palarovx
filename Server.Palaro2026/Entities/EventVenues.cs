using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class EventVenues
{
    public int ID { get; set; }

    public string? Address { get; set; }

    public string? Venue { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual ICollection<Events> Events { get; set; } = new List<Events>();
}
