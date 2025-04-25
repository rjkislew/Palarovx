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

    public string? PasswordHash { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? Active { get; set; }

    public int? RoleID { get; set; }

    [JsonIgnore]
    public virtual ICollection<Events> Events { get; set; } = new List<Events>();

    public virtual UserRoles? Role { get; set; }
}
