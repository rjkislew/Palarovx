using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using System.Text.Json;

namespace Server.Palaro2026.Controller
{
    public partial class UsersController : ControllerBase
    {
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

        [HttpGet("{id}")] // GET: api/Users/{id}
        public async Task<ActionResult<UsersDTO.Users>> GetUser(string id)
        {
            var user = await _context.Users
                .Where(u => u.ID == id)
                .Select(x => UsersDTOMapper(x))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost] // POST: api/Users
        public async Task<ActionResult<Users>> PostUsers(UsersDTO.Users users)
        {
            var usersDTO = new Users
            {
                ID = users.ID,
                FirstName = users.FirstName,
                LastName = users.LastName,
                Username = users.Username,
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
    }
}
