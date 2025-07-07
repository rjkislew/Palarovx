using Server.Palaro2026.Entities;

namespace Server.Palaro2026.DTO;

public class UsersDTO
{
    public class UserDetails
    {
        public string ID { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Affiliation { get; set; }
        public string? EmailAddress { get; set; }
        public string? ContactNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool? Active { get; set; }
        public string? Role { get; set; }
    }

    public class UsernameList
    {
        public string? Username { get; set; }
    }

    public class UserList
    {
        public string ID { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public bool? Active { get; set; }
    }

    public class UserLogin
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Active { get; set; }
    }

    public class UserRegistration
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
        public bool? Active { get; set; }
    }

    public class JWTUserAuthentication
    {
        public string ID { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
    }

    public class Users
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
    }

    public class UserRoles
    {
        public int ID { get; set; }
        public string? Role { get; set; }
        public string? Description { get; set; }
    }
}
