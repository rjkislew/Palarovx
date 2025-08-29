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
