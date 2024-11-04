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
    public class UsersController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        /// 
        /// 
        /// Users
        /// 
        /// 

        // Create
        [HttpPost("User")]
        public async Task<ActionResult<UsersDTO.UsersDetails.UD_Users>> CreateUser([FromBody] UsersDTO.UsersDetails.UD_Users usersContent)
        {
            try
            {
                // Hash the password
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(usersContent.PasswordHash);
                    var hashedBytes = sha256.ComputeHash(bytes);
                    usersContent.PasswordHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }

                var users = new Users
                {
                    ID = usersContent.ID,
                    Username = usersContent.Username,
                    Email = usersContent.Email,
                    PasswordHash = usersContent.PasswordHash, // Save the hashed password
                    CreatedAt = usersContent.CreatedAt,
                    UpdateAt = usersContent.UpdateAt,
                    LastLogin = usersContent.LastLogin,
                    Active = usersContent.Active,
                    RoleID = usersContent.RoleID,
                };

                _context.Users.Add(users);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUsers), new { id = users.ID }, usersContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UsersDTO.UserLoginDetails.ULD_Users>> Login([FromBody] UsersDTO.UserLoginDetails.ULD_Users loginContent)
        {
            try
            {
                // Hash the input password for comparison
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(loginContent.PasswordHash);
                    var hashedBytes = sha256.ComputeHash(bytes);
                    var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

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

                    // Prepare the response object (you might want to limit the details returned)
                    var userDto = new UsersDTO.UsersDetails.UD_Users
                    {
                        ID = user.ID,
                        Username = user.Username,
                        Email = user.Email,
                        LastLogin = user.LastLogin,
                        Active = user.Active,
                        RoleID = user.RoleID
                    };

                    return Ok(userDto);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        // Read
        [HttpGet("User")]
        public async Task<ActionResult<IEnumerable<UsersDTO.UsersDetails.UD_Users>>> GetUsers()
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


        //Update
        [HttpPut("User/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UsersDTO.UserUpdateDetails.UUD_Users usersContent)
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
    }
}