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
        // Map ProfileCoaches to ProfilesDTO.ProfileCoaches
        private static ProfilesDTO.ProfileCoaches ProfileCoachesDTOMapper(ProfileCoaches profileCoaches) =>
           new ProfilesDTO.ProfileCoaches
           {
               ID = profileCoaches.ID,
               FirstName = profileCoaches.FirstName,
               LastName = profileCoaches.LastName,
               SchoolRegionID = profileCoaches.SchoolRegionID,
           };

        [HttpGet("Coach")] // /api/Profiles/Coach
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfileCoaches>>> GetProfileCoaches(
        [FromQuery] string? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolRegionID = null)
        {
            var query = _context.ProfileCoaches.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName!.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName!.Contains(lastName));

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            return await query
                .Select(x => ProfileCoachesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Coach")] // /api/Profiles/Coach
        public async Task<ActionResult<ProfileCoaches>> PostProfileCoaches(ProfilesDTO.ProfileCoaches profileCoaches)
        {
            var profileCoachesDTO = new ProfileCoaches
            {
                ID = profileCoaches.ID,
                FirstName = profileCoaches.FirstName,
                LastName = profileCoaches.LastName,
                SchoolRegionID = profileCoaches.SchoolRegionID,
            };

            _context.ProfileCoaches.Add(profileCoachesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfileCoachesExist(profileCoaches.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfileCoaches", new { id = profileCoaches.ID }, ProfileCoachesDTOMapper(profileCoachesDTO));
        }

        [HttpPut("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> PutProfileCoaches(string id, ProfilesDTO.ProfileCoaches profileCoaches)
        {
            if (id != profileCoaches.ID)
            {
                return BadRequest();
            }

            var existingCoachProfile = await _context.ProfileCoaches.FindAsync(id);
            if (existingCoachProfile == null)
            {
                return NotFound();
            }

            existingCoachProfile.FirstName = profileCoaches.FirstName;
            existingCoachProfile.LastName = profileCoaches.LastName;
            existingCoachProfile.SchoolRegionID = profileCoaches.SchoolRegionID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileCoachesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> PatchProfileCoaches(string id, [FromBody] ProfilesDTO.ProfileCoaches updatedProfileCoach)
        {
            var existingCoach = await _context.ProfileCoaches.FindAsync(id);

            if (existingCoach == null) return NotFound();

            if (updatedProfileCoach.FirstName != null) existingCoach.FirstName = updatedProfileCoach.FirstName;
            if (updatedProfileCoach.LastName != null) existingCoach.LastName = updatedProfileCoach.LastName;
            if (updatedProfileCoach.SchoolRegionID != null) existingCoach.SchoolRegionID = updatedProfileCoach.SchoolRegionID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileCoachesExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfileCoachesExist(updatedProfileCoach.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> DeleteProfileCoaches(int id)
        {
            var ProfileCoaches = await _context.ProfileCoaches.FindAsync(id);
            if (ProfileCoaches == null)
            {
                return NotFound();
            }

            _context.ProfileCoaches.Remove(ProfileCoaches);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a ProfileCoach exist by ID
        private bool ProfileCoachesExist(string id)
        {
            return _context.ProfileCoaches.Any(e => e.ID == id);
        }
    }
}
