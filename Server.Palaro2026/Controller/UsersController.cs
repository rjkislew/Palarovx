using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using System.Text.Json;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public UsersController(Palaro2026Context context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------------------------------------------------------

        // User Views

        // this method is used to get the user details by userID (single user)
        [HttpGet("UserDetails")] // GET: api/Users/UserDetails?userID={userID}
        public async Task<ActionResult<UsersDTO.UserDetails>> GetUserDetails(string? userID)
        {
            var user = await _context.Users
                .Include(r => r.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ID == userID);

            if (user == null)
                return NotFound();

            var mappedUser = new UsersDTO.UserDetails
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Affiliation = user.Affiliation,
                EmailAddress = user.EmailAddress,
                ContactNumber = user.ContactNumber,
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt,
                LastLogin = user.LastLogin,
                Active = user.Active,
                Role = user.Role?.Role
            };

            return Ok(mappedUser);
        }

        // this method is used to get the list of all users with their details
        [HttpGet("UsersDetails")] // GET: api/Users/UsersDetails
        public async Task<ActionResult<IEnumerable<UsersDTO.UserDetails>>> GetUsersInformation()
        {
            var users = await _context.Users
                .Include(r => r.Role)
                .AsNoTracking()
                .ToListAsync();

            var mappedUsers = users.Select(user => new UsersDTO.UserDetails
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Affiliation = user.Affiliation,
                EmailAddress = user.EmailAddress,
                ContactNumber = user.ContactNumber,
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt,
                LastLogin = user.LastLogin,
                Active = user.Active,
                Role = user.Role?.Role
            }).ToList();

            return Ok(mappedUsers);
        }

        // this method is used to get the list of all tally clerks 
        [HttpGet("TallyClerkList")] // GET: api/Users/TallyClerkList
        public async Task<ActionResult<IEnumerable<UsersDTO.UserList>>> GetTallyClerkList()
        {
            var users = await _context.Users
                .Include(r => r.Role)
                .Where(user => user.Role != null && user.Role.Role == "Tally Clerk")
                .AsNoTracking()
                .ToListAsync();

            var mappedUsers = users.Select(user => new UsersDTO.UserList
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role?.Role
            }).ToList();

            return Ok(mappedUsers);
        }
        
        // this method is used to get the list of all usernames
        [HttpGet("Usernames")] // GET: api/Users/Usernames
        public async Task<ActionResult<IEnumerable<UsersDTO.UsernameList>>> GetUsernamesList()
        {
            var users = await _context.Users
                .Include(r => r.Role)
                .AsNoTracking()
                .ToListAsync();

            var mappedUsers = users.Select(user => new UsersDTO.UsernameList
            {
                Username = user.Username,
            }).ToList();

            return Ok(mappedUsers);
        }

        // ------------------------------------------------------------------------------------------------------------------

        // User REST Methods

        // Password Hashing Method
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static UsersDTO.Users UsersDTOMapper(Users users) =>
           new UsersDTO.Users
           {
               ID = users.ID,
               FirstName = users.FirstName,
               LastName = users.LastName,
               Username = users.Username,
               Affiliation = users.Affiliation,
               EmailAddress = users.EmailAddress,
               ContactNumber = users.ContactNumber,
               PasswordHash = users.PasswordHash,
               CreatedAt = users.CreatedAt,
               UpdateAt = users.UpdateAt,
               LastLogin = users.LastLogin,
               Active = users.Active,
               RoleID = users.RoleID,
           };

        [HttpGet] // GET: api/Users
        public async Task<ActionResult<IEnumerable<UsersDTO.Users>>> GetUsers([FromQuery] string? userID)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(userID))
            {
                query = query.Where(u => u.ID == userID);
            }

            var result = await query
                .Select(x => UsersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost] // POST: api/Users
        public async Task<ActionResult<Users>> PostUsers(UsersDTO.Users users)
        {
            // Generate username in lowercase
            string username = $"{users.FirstName?.Replace(" ", "")}.{users.LastName}".ToLower();

            var usersDTO = new Users
            {
                ID = users.ID,
                FirstName = users.FirstName,
                LastName = users.LastName,
                Username = username,
                Affiliation = users.Affiliation,
                EmailAddress = users.EmailAddress,
                ContactNumber = users.ContactNumber,
                CreatedAt = users.CreatedAt,
                UpdateAt = users.UpdateAt,
                LastLogin = users.LastLogin,
                Active = users.Active,
                RoleID = users.RoleID,
            };

            // Only hash and set the password if it's not null or empty
            if (!string.IsNullOrWhiteSpace(users.PasswordHash))
            {
                usersDTO.PasswordHash = HashPassword(users.PasswordHash);
            }

            _context.Users.Add(usersDTO);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExist(users.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUsers", new { id = users.ID }, UsersDTOMapper(usersDTO));
        }

        [HttpPut("{id}")] // PUT: api/Users/{id}
        public async Task<IActionResult> PutUsers(string id, UsersDTO.Users userDto)
        {
            if (id != userDto.ID)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FirstName = userDto.FirstName;
            existingUser.LastName = userDto.LastName;
            existingUser.Username = userDto.Username;
            existingUser.Affiliation = userDto.Affiliation;
            existingUser.EmailAddress = userDto.EmailAddress;
            existingUser.ContactNumber = userDto.ContactNumber;
            existingUser.PasswordHash = userDto.PasswordHash;
            existingUser.CreatedAt = userDto.CreatedAt;
            existingUser.UpdateAt = userDto.UpdateAt;
            existingUser.LastLogin = userDto.LastLogin;
            existingUser.Active = userDto.Active;
            existingUser.RoleID = userDto.RoleID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("{id}")] // PATCH: api/Users/{id}
        public async Task<IActionResult> PatchUsers(string id, [FromBody] UsersDTO.Users updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (updatedUser.FirstName != null) user.FirstName = updatedUser.FirstName;
            if (updatedUser.LastName != null) user.LastName = updatedUser.LastName;
            if (updatedUser.Username != null) user.Username = updatedUser.Username;
            if (updatedUser.Affiliation != null) user.Affiliation = updatedUser.Affiliation;
            if (updatedUser.EmailAddress != null) user.EmailAddress = updatedUser.EmailAddress;
            if (updatedUser.ContactNumber != null) user.ContactNumber = updatedUser.ContactNumber;
            if (updatedUser.PasswordHash != null) user.PasswordHash = HashPassword(updatedUser.PasswordHash);
            if (updatedUser.CreatedAt != null) user.CreatedAt = updatedUser.CreatedAt;
            if (updatedUser.UpdateAt != null) user.UpdateAt = updatedUser.UpdateAt;
            if (updatedUser.LastLogin != null) user.LastLogin = updatedUser.LastLogin;
            if (updatedUser.Active != null) user.Active = updatedUser.Active;
            if (updatedUser.RoleID != null) user.RoleID = updatedUser.RoleID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (UserExist(updatedUser.ID)) return Conflict();
                else throw;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")] // DELETE: api/Users/{id}
        public async Task<IActionResult> DeleteUsers(string id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // this method checks if a user exists by ID
        private bool UserExist(string id)
        {
            return _context.Users.Any(e => e.ID == id);
        }

        // ------------------------------------------------------------------------------------------------------------------

        // User Roles REST Methods

        private static UsersDTO.UserRoles UserRolesDTOMapper(UserRoles userRoles) =>
           new UsersDTO.UserRoles
           {
               ID = userRoles.ID,
               Role = userRoles.Role,
               Description = userRoles.Description,
           };

        [HttpGet("Roles")] // GET: api/Users/Roles
        public async Task<ActionResult<IEnumerable<UsersDTO.UserRoles>>> GetUserRoles()
        {
            return await _context.UserRoles
                .Select(x => UserRolesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Roles")] // POST: api/Users/Roles
        public async Task<ActionResult<UserRoles>> PostUserRoles(UsersDTO.UserRoles userRoles)
        {
            var userRolesDTO = new UserRoles
            {
                ID = userRoles.ID,
                Role = userRoles.Role,
                Description = userRoles.Description,
            };

            _context.UserRoles.Add(userRolesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserRoleExist(userRoles.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUserRoles", new { id = userRoles.ID }, UserRolesDTOMapper(userRolesDTO));
        }

        [HttpPut("Roles/{id}")] // PUT: api/Users/Roles/{id}
        public async Task<IActionResult> PutUserRoles(int id, UsersDTO.UserRoles userRoles)
        {
            if (id != userRoles.ID)
            {
                return BadRequest();
            }

            var existingUserRole = await _context.UserRoles.FindAsync(id);
            if (existingUserRole == null)
            {
                return NotFound();
            }

            existingUserRole.Role = userRoles.Role;
            existingUserRole.Description = userRoles.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserRoleExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Roles/{id}")] // PATCH: api/Users/Roles/{id}
        public async Task<IActionResult> PatchUsersRoles(int id, [FromBody] UsersDTO.UserRoles updatedRole)
        {
            var user = await _context.UserRoles.FindAsync(id);
            if (user == null) return NotFound();

            if (updatedRole.Role != null) user.Role = updatedRole.Role;
            if (updatedRole.Description != null) user.Description = updatedRole.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserRoleExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (UserRoleExist(updatedRole.ID)) return Conflict();
                else throw;
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("Roles/{id}")] // DELETE: api/Users/Roles/{id}
        public async Task<IActionResult> DeleteUserRoles(int id)
        {
            var userRoles = await _context.UserRoles.FindAsync(id);
            if (userRoles == null)
            {
                return NotFound();
            }

            _context.UserRoles.Remove(userRoles);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // this method checks if a user role exists by ID
        private bool UserRoleExist(int id)
        {
            return _context.UserRoles.Any(e => e.ID == id);
        }
    }
}
