namespace Server.Palaro2026.DTO;

public class UsersDTO
{
    public class UsersDetails
    {
        public partial class UD_Users
        {
            public int ID { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? PasswordHash { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdateAt { get; set; }
            public DateTime? LastLogin { get; set; }
            public bool? Active { get; set; }
            public int? RoleID { get; set; }
        }
    }

    public class UserLoginDetails
    {
        public partial class ULD_Users
        {
            public string? Username { get; set; }
            public string? PasswordHash { get; set; }
            public DateTime? LastLogin { get; set; }
        }
    }

    public class UserUpdateDetails
    {
        public partial class UUD_Users
        {
            public int ID { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? PasswordHash { get; set; }
            public DateTime? UpdateAt { get; set; }
            public DateTime? LastLogin { get; set; }
            public bool? Active { get; set; }
            public int? RoleID { get; set; }
        }
    }
}