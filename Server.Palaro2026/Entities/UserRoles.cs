using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class UserRoles
{
    public int ID { get; set; }

    public string? Role { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}
