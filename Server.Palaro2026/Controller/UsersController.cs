using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsersDTO.Users>> GetUsers(string id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return UsersDTOMapper(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(string id, UsersDTO.Users users)
        {
            if (id != users.ID)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
                .ToListAsync();
        }

        [HttpGet("Roles/{id}")]
        public async Task<ActionResult<UsersDTO.UserRoles>> GetUserRoles(int id)
        {
            var userRoles = await _context.UserRoles.FindAsync(id);

            if (userRoles == null)
            {
                return NotFound();
            }

            return UserRolesDTOMapper(userRoles);
        }

        [HttpPut("Roles/{id}")]
        public async Task<IActionResult> PutUserRoles(int id, UsersDTO.UserRoles userRoles)
        {
            if (id != userRoles.ID)
            {
                return BadRequest();
            }

            _context.Entry(userRoles).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
