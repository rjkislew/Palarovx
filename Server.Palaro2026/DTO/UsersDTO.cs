namespace Server.Palaro2026.DTO;

public class UsersDTO
{
    public class UserSessionID
    {
        public string? ID { get; set; }
        public string? SessionID { get; set; }
        public string? RecentIP { get; set; }
    }

    public class UserLoginDetails
    {
        public partial class ULD_UsersContent
        {
            public string? Username { get; set; }
            public string? PasswordHash { get; set; }
            public string? SessionID { get; set; }
            public string? RecentIP { get; set; }
            public DateTime? LastLogin { get; set; }
        }
    }

    public class UserUpdateDetails
    {
        public partial class UUD_UsersContent
        {
            public string? ID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? PasswordHash { get; set; }
            public DateTime? UpdateAt { get; set; }
            public DateTime? LastLogin { get; set; }
            public bool? Active { get; set; }
            public int? RoleID { get; set; }
        }
    }

    public class UsersDetails
    {
        public class UD_RolesContent
        {
            public string? Role { get; set; }
            public List<UD_UsersContent>? UserList { get; set; }
        }

        public class UD_UsersContent
        {
            public string? ID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? PasswordHash { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdateAt { get; set; }
            public DateTime? LastLogin { get; set; }
            public bool? Active { get; set; }
        }
    }

    public class UserDetails
    {
        public class UD_UserContent
        {
            public string? Role { get; set; }
            public string? ID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? PasswordHash { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdateAt { get; set; }
            public DateTime? LastLogin { get; set; }
            public bool? Active { get; set; }
        }
    }

    public class Users
    {
        public class UsersContent
        {
            public string? ID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? PasswordHash { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdateAt { get; set; }
            public DateTime? LastLogin { get; set; }
            public bool? Active { get; set; }
            public string? SessionID { get; set; }
            public int? RoleID { get; set; }
        }

        public class UserID
        {
            public string? ID { get; set; }
        }
    }

    public class UserRoles
    {
        public partial class UserRolesContent
        {
            public int ID { get; set; }

            public string? Role { get; set; }

            public string? Description { get; set; }
        }
    }
}