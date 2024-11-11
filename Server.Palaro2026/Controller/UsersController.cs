using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(Palaro2026Context context, TokenService tokenService) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;
        private readonly TokenService _tokenServiceon = tokenService;

        [HttpPost("Login")]
        public async Task<ActionResult<UsersDTO.Users.UserID>> Login([FromBody] UsersDTO.UserLoginDetails.ULD_UsersContent loginContent)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(loginContent.Username) || string.IsNullOrWhiteSpace(loginContent.PasswordHash))
                {
                    return BadRequest("Username and password are required.");
                }

                // Hash the input password for comparison
                string hashedPassword;
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(loginContent.PasswordHash);
                    var hashedBytes = sha256.ComputeHash(bytes);
                    hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }

                // Find the user by username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginContent.Username);

                // Check if user exists and password matches
                if (user == null || user.PasswordHash != hashedPassword)
                {
                    return Unauthorized("Invalid username or password.");
                }

                // Update last login time
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT token with user ID
                var token = tokenService.GenerateToken(user.Username, user.ID.ToString()); // Pass user ID for uniqueness

                // Return the user's ID and token upon successful login
                var userID = new UsersDTO.Users.UserID
                {
                    ID = user.ID // Assuming 'user.ID' is the field representing the user's ID
                };

                return Ok(new { UserID = userID, Token = token }); // Return the UserID and Token
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        /// 
        /// 
        /// VIEW
        /// 
        /// 

        [HttpGet("UsersDetails")]
        public async Task<ActionResult<IEnumerable<UsersDTO.UsersDetails.UD_RolesContent>>> GetUsersDetails()
        {
            try
            {
                // Fetch the data from the database
                var users = await _context.UsersDetails
                    .AsNoTracking()
                    .ToListAsync();

                // Group the users by roles
                var groupedUsers = users
                    .GroupBy(c => c.Role)
                    .Select(role => new UsersDTO.UsersDetails.UD_RolesContent
                    {
                        Role = role.Key,
                        UserList = role
                        .Select(user => new UsersDTO.UsersDetails.UD_UsersContent
                        {
                            ID = user.ID,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Username = user.Username,
                            Email = user.Email,
                            PasswordHash = user.PasswordHash,
                            CreatedAt = user.CreatedAt,
                            UpdateAt = user.UpdateAt,
                            LastLogin = user.LastLogin,
                            Active = user.Active,
                        }).ToList()
                    }).ToList();

                return Ok(groupedUsers);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("UserDetails")]
        public async Task<ActionResult<IEnumerable<UsersDTO.UserDetails.UD_UserContent>>> GetUserDetails(string searchTerm = null)
        {
            try
            {
                var usersQuery = _context.UsersDetails.AsNoTracking().AsQueryable();

                // Apply filtering if searchTerm is provided, focusing only on the ID field
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Use EF.Functions.Like for case-insensitive searching based on ID
                    usersQuery = usersQuery.Where(user =>
                        EF.Functions.Like(user.ID.ToString(), $"%{searchTerm.ToLower()}%"));
                }

                // Return the entire list of users (no projection)
                var users = await usersQuery.ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        /// 
        /// 
        /// USERS
        /// 
        /// 


        // Helper method to generate a 20-character uppercase ID
        private string GenerateRandomId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var id = new string(Enumerable.Range(0, 20)
                                           .Select(_ => chars[random.Next(chars.Length)])
                                           .ToArray());
            return id;
        }

        // Helper method to hash the password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashedBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Create
        [HttpPost("User")]
        public async Task<ActionResult<UsersDTO.Users.UsersContent>> CreateUser([FromBody] UsersDTO.Users.UsersContent usersContent)
        {
            try
            {
                // Generate the ID if not provided
                if (string.IsNullOrEmpty(usersContent.ID))
                {
                    usersContent.ID = GenerateRandomId();
                }

                // Hash the password
                usersContent.PasswordHash = HashPassword(usersContent.PasswordHash);

                // Map the UsersContent to Users entity
                var users = new Users
                {
                    ID = usersContent.ID,
                    FirstName = usersContent.FirstName,
                    LastName = usersContent.LastName,
                    Username = usersContent.Username,
                    Email = usersContent.Email,
                    PasswordHash = usersContent.PasswordHash, // Save the hashed password
                    CreatedAt = usersContent.CreatedAt,
                    UpdateAt = usersContent.UpdateAt,
                    LastLogin = usersContent.LastLogin,
                    Active = usersContent.Active,
                    RoleID = usersContent.RoleID,
                };

                // Add the new user to the database
                _context.Users.Add(users);
                await _context.SaveChangesAsync();

                // Return the created user with a location header
                return CreatedAtAction(nameof(GetUsers), new { id = users.ID }, usersContent);
            }
            catch (Exception ex)
            {
                // Return 500 Internal Server Error with exception details
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("User")]
        public async Task<ActionResult<IEnumerable<UsersDTO.Users.UsersContent>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.AsNoTracking().ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("User/{id}")]
        public async Task<IActionResult> UpdateUser(string id, UsersDTO.UserUpdateDetails.UUD_UsersContent usersContent)
        {
            if (id != usersContent.ID)
            {
                return BadRequest("User ID mismatch");
            }

            try
            {
                // Find the existing user
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                // Update only the fields that are not null
                existingUser.FirstName = usersContent.FirstName ?? existingUser.FirstName;
                existingUser.LastName = usersContent.LastName ?? existingUser.LastName;
                existingUser.Username = usersContent.Username ?? existingUser.Username;
                existingUser.Email = usersContent.Email ?? existingUser.Email;

                // Update password only if it is provided
                if (!string.IsNullOrWhiteSpace(usersContent.PasswordHash))
                {
                    using (var sha256 = SHA256.Create())
                    {
                        var bytes = Encoding.UTF8.GetBytes(usersContent.PasswordHash);
                        var hashedBytes = sha256.ComputeHash(bytes);
                        existingUser.PasswordHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                    }
                }

                existingUser.UpdateAt = usersContent.UpdateAt ?? existingUser.UpdateAt;
                existingUser.LastLogin = usersContent.LastLogin ?? existingUser.LastLogin;
                existingUser.Active = usersContent.Active ?? existingUser.Active;
                existingUser.RoleID = usersContent.RoleID ?? existingUser.RoleID;

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.ID == id))
                {
                    return NotFound($"User with ID {id} not found");
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("User/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        /// 
        /// 
        /// ROLES
        /// 
        /// 

        // Create
        [HttpPost("UserRoles")]
        public async Task<ActionResult<UsersDTO.UserRoles.UserRolesContent>> CreateRole([FromBody] UsersDTO.UserRoles.UserRolesContent rolesContent)
        {
            try
            {
                var role = new UserRoles
                {
                    ID = rolesContent.ID,
                    Description = rolesContent.Description,
                };

                _context.UserRoles.Add(role);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRoles), new { id = role.ID }, rolesContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("UserRoles")]
        public async Task<ActionResult<IEnumerable<UsersDTO.UserRoles.UserRolesContent>>> GetRoles()
        {
            try
            {
                var roles = await _context.UserRoles.AsNoTracking().ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("UserRoles/{id}")]
        public async Task<IActionResult> UpdateRoles(int id, UsersDTO.UserRoles.UserRolesContent rolesContent)
        {
            if (id != rolesContent.ID)
            {
                return BadRequest("Role ID mismatch");
            }

            try
            {
                var role = new UserRoles
                {
                    ID = rolesContent.ID,
                    Description = rolesContent.Description,
                };

                _context.UserRoles.Attach(role);
                _context.Entry(role).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UserRoles.Any(e => e.ID == id))
                {
                    return NotFound($"Role with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("UserRoles/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.UserRoles.FindAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found");
            }

            _context.UserRoles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}