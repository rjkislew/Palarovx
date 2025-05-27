using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

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

        [HttpGet("UserDetail")]
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
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt,
                LastLogin = user.LastLogin,
                Active = user.Active,
                Role = user.Role?.Role
            };

            return Ok(mappedUser);
        }


        [HttpGet("UsersInformation")]
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
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt,
                LastLogin = user.LastLogin,
                Active = user.Active,
                Role = user.Role?.Role
            }).ToList();

            return Ok(mappedUsers);
        }

        [HttpGet("TallyClerkList")]
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

        [HttpGet("Usernames")]
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

        private static UsersDTO.Users UsersDTOMapper(Users users) =>
           new UsersDTO.Users
           {
               ID = users.ID,
               FirstName = users.FirstName,
               LastName = users.LastName,
               Username = users.Username,
               PasswordHash = users.PasswordHash,
               CreatedAt = users.CreatedAt,
               UpdateAt = users.UpdateAt,
               LastLogin = users.LastLogin,
               Active = users.Active,
               RoleID = users.RoleID,
           };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsersDTO.Users>>> GetUsers()
        {
            return await _context.Users
                .Select(x => UsersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(string id, UsersDTO.Users users)
        {
            if (id != users.ID)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FirstName = users.FirstName;
            existingUser.LastName = users.LastName;
            existingUser.Username = users.Username;
            existingUser.PasswordHash = users.PasswordHash;
            existingUser.CreatedAt = users.CreatedAt;
            existingUser.UpdateAt = users.UpdateAt;
            existingUser.LastLogin = users.LastLogin;
            existingUser.Active = users.Active;
            existingUser.RoleID = users.RoleID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Users>> PostUsers(UsersDTO.Users users)
        {
            var usersDTO = new Users
            {
                ID = users.ID,
                FirstName = users.FirstName,
                LastName = users.LastName,
                Username = users.Username,
                PasswordHash = users.PasswordHash,
                CreatedAt = users.CreatedAt,
                UpdateAt = users.UpdateAt,
                LastLogin = users.LastLogin,
                Active = users.Active,
                RoleID = users.RoleID,
            };

            _context.Users.Add(usersDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UsersExists(users.ID))
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

        [HttpDelete("{id}")]
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

        private bool UsersExists(string id)
        {
            return _context.Users.Any(e => e.ID == id);
        }




        // User Roles

        private static UsersDTO.UserRoles UserRolesDTOMapper(UserRoles userRoles) =>
           new UsersDTO.UserRoles
           {
               ID = userRoles.ID,
               Role = userRoles.Role,
               Description = userRoles.Description,
           };

        [HttpGet("Roles")]
        public async Task<ActionResult<IEnumerable<UsersDTO.UserRoles>>> GetUserRoles()
        {
            return await _context.UserRoles
                .Select(x => UserRolesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Roles/{id}")]
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
                if (!UserRolesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Roles")]
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
                if (UserRolesExists(userRoles.ID))
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

        [HttpDelete("Roles/{id}")]
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

        private bool UserRolesExists(int id)
        {
            return _context.UserRoles.Any(e => e.ID == id);
        }
    }
}
