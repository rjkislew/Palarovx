using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class ProfilesController
    {
        // Profile Player Sport Coaches
        private static ProfilesDTO.ProfilePlayerSportCoaches ProfilePlayerSportCoachesDTOMapper(ProfilePlayerSportCoaches profilePlayerSportCoaches) =>
           new ProfilesDTO.ProfilePlayerSportCoaches
           {
               ID = profilePlayerSportCoaches.ID,
               ProfileCoachID = profilePlayerSportCoaches.ProfileCoachID,
               ProfilePlayerSportID = profilePlayerSportCoaches.ProfilePlayerSportID
           };

        [HttpGet("Player/Sports/Coaches")] // /api/Profiles/Player/Sports/Coaches
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayerSportCoaches>>> GetProfilePlayerSportCoaches(
        [FromQuery] int? ID = null,
        [FromQuery] string? profileCoachID = null,
        [FromQuery] int? profilePlayerSportID = null)
        {
            var query = _context.ProfilePlayerSportCoaches.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(profileCoachID))
                query = query.Where(x => x.ProfileCoachID == profileCoachID);

            if (profilePlayerSportID.HasValue)
                query = query.Where(x => x.ProfilePlayerSportID == profilePlayerSportID.Value);

            return await query
                .AsNoTracking()
                .Select(x => ProfilePlayerSportCoachesDTOMapper(x))
                .ToListAsync();
        }

        [HttpPost("Player/Sports/Coaches")] // /api/Profiles/Player/Sports/Coaches
        public async Task<ActionResult> AddProfilePlayerSportCoaches([FromBody] List<ProfilesDTO.ProfilePlayerSportCoaches> coaches)
        {
            if (coaches == null || !coaches.Any())
            {
                return BadRequest("No coaches provided.");
            }

            var profilePlayerSportCoachesList = coaches.Select(coach => new ProfilePlayerSportCoaches
            {
                ProfileCoachID = coach.ProfileCoachID,
                ProfilePlayerSportID = coach.ProfilePlayerSportID
            }).ToList();

            _context.ProfilePlayerSportCoaches.AddRange(profilePlayerSportCoachesList);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{coaches.Count} coaches added successfully." });
        }

        [HttpPut("Player/Sports/Coaches/{id}")] // /api/Profiles/Player/Sports/Coaches/{id}
        public async Task<IActionResult> PutProfilePlayerSportCoaches(int id, ProfilesDTO.ProfilePlayerSportCoaches profilePlayerSportCoaches)
        {
            if (id != profilePlayerSportCoaches.ID)
            {
                return BadRequest();
            }

            var existingProfilePlayerSportCoach = await _context.ProfilePlayerSportCoaches.FindAsync(id);
            if (existingProfilePlayerSportCoach == null)
            {
                return NotFound();
            }

            existingProfilePlayerSportCoach.ProfileCoachID = profilePlayerSportCoaches.ProfileCoachID;
            existingProfilePlayerSportCoach.ProfilePlayerSportID = profilePlayerSportCoaches.ProfilePlayerSportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayerSportCoachesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Player/Sports/Coaches/{id}")] // /api/Profiles/Player/Sports/Coaches/{id}
        public async Task<IActionResult> PatchProfilePlayerSportCoaches(int id, [FromBody] ProfilesDTO.ProfilePlayerSportCoaches updatedPlayerSportCoach)
        {
            var existingCoach = await _context.ProfilePlayerSportCoaches.FindAsync(id);

            if (existingCoach == null) return NotFound();

            if (updatedPlayerSportCoach.ProfileCoachID != null) existingCoach.ProfileCoachID = updatedPlayerSportCoach.ProfileCoachID;
            if (updatedPlayerSportCoach.ProfilePlayerSportID != null) existingCoach.ProfilePlayerSportID = updatedPlayerSportCoach.ProfilePlayerSportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProfilePlayerSportCoaches.Any(e => e.ID == id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (_context.ProfilePlayerSportCoaches.Any(e => e.ID == updatedPlayerSportCoach.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Player/Sports/Coaches/{id}")] // /api/Profiles/Player/Sports/Coaches/{id}
        public async Task<IActionResult> DeleteProfilePlayerSportCoaches(int id)
        {
            var profilePlayerSportCoaches = await _context.ProfilePlayerSportCoaches.FindAsync(id);
            if (profilePlayerSportCoaches == null)
            {
                return NotFound();
            }

            _context.ProfilePlayerSportCoaches.Remove(profilePlayerSportCoaches);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Delete ProfilePlayerSportCoaches by PlayerSportID
        [HttpDelete("Player/Sports/Coaches/ByPlayerSport/{playerSportID}")] // /api/Profiles/Player/Sports/Coaches/ByPlayerSport/{playerSportID}
        public async Task<IActionResult> DeleteByProfilePlayerSportID(int playerSportID)
        {
            var recordsToDelete = _context.ProfilePlayerSportCoaches
                .Where(p => p.ProfilePlayerSportID == playerSportID)
                .ToList();

            if (!recordsToDelete.Any())
            {
                return NotFound(new { Message = "No records found for the given PlayerSportID." });
            }

            _context.ProfilePlayerSportCoaches.RemoveRange(recordsToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{recordsToDelete.Count} records deleted successfully." });
        }

        // Check if a ProfilePlayerSportCoach exists by ID
        private bool ProfilePlayerSportCoachesExist(int id)
        {
            return _context.ProfilePlayerSportCoaches.Any(e => e.ID == id);
        }
    }
}
