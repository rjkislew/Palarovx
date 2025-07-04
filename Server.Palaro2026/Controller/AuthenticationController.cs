using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Palaro2026Context _context;

        public AuthenticationController(Palaro2026Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Registration

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Register user
        [HttpPost("Register")]
        public async Task<ActionResult<Users>> PostUsers(UsersDTO.UserRegistration users)
        {
            // Hash the password
            var hashedPassword = HashPassword(users.PasswordHash!);

            // Generate username in lowercase
            string username = $"{users.FirstName?.Replace(" ", "")}.{users.LastName}".ToLower();

            var usersDTO = new Users
            {
                ID = users.ID,
                FirstName = users.FirstName,
                LastName = users.LastName,
                Username = username,
                PasswordHash = hashedPassword, // Make sure to store the hashed password
                CreatedAt = DateTime.UtcNow,
                Active = true,
            };

            // Check if the user exists before adding
            if (UserExist(users.ID))
            {
                return Conflict();  // Return a conflict if the user already exists
            }

            _context.Users.Add(usersDTO);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database exception: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

            // Return the created user as a response
            return Ok();
        }

        // Check if user exists
        private bool UserExist(string id)
        {
            return _context.Users.Any(e => e.ID == id);
        }


        // ------------------------------------------------------------------------------------------------------------------

        // Login

        // Verify password using BCrypt
        private bool VerifyPassword(string inputPassword, string storedHashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHashedPassword);
        }

        // Generate JWT token 
        private string GenerateJwtToken(UsersDTO.JWTUserAuthentication user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var keyString = jwtSettings["Key"] ?? throw new ArgumentNullException("JWT Key is missing in configuration.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define the current time in UTC+8
            var utcPlus8 = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.ID),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Iat, utcPlus8.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("Role", user.Role ?? string.Empty)
            };

            // Expiration is still in UTC, so use DateTime.UtcNow.AddHours(12)
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12), // Token valid for 12 hours
                signingCredentials: credentials
            );

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }

        // Login user
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UsersDTO.UserLogin loginRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    return BadRequest(new { message = "Email address and password are required." });
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

                if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash!))
                {
                    return Unauthorized(new { message = "Invalid email address or password." });
                }

                // Update last login timestamp
                user.LastLogin = DateTime.UtcNow;
                _context.Users.Update(user); 
                await _context.SaveChangesAsync();

                // Map Users entity to UserDTO.JWTUserAuthentication.User
                var userDto = new UsersDTO.JWTUserAuthentication
                {
                    ID = user.ID.ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role?.Role,
                };

                var token = GenerateJwtToken(userDto);

                return Ok(new
                {
                    token = token,
                    id = user.ID,
                    role = user.Role?.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Internal server error.",
                    error = ex.Message
                });
            }
        }
    }
}
