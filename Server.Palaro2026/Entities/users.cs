using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Palaro2026.Entities;

public partial class Users
{
    public string ID { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? Affiliation { get; set; }

    public string? EmailAddress { get; set; }

    public string? ContactNumber { get; set; }

    public string? PasswordHash { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? Active { get; set; }

    public int? RoleID { get; set; }

    [JsonIgnore]
    public virtual ICollection<Events> Events { get; set; } = new List<Events>();

    [JsonIgnore]
    public virtual ICollection<News> News { get; set; } = new List<News>();
    [JsonIgnore]
    public virtual ICollection<ProfilePlayers> ProfilePlayers { get; set; } = new List<ProfilePlayers>();
    [JsonIgnore]
    public virtual ICollection<ProfileCoaches> ProfileCoaches { get; set; } = new List<ProfileCoaches>();

    public virtual UserRoles? Role { get; set; }
}
