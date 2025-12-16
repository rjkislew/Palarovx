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
               MiddleInitial = profileCoaches.MiddleInitial,
               Sex = profileCoaches.Sex,
               BirthDate = profileCoaches.BirthDate,
               Designation = profileCoaches.Designation,
               SportID = profileCoaches.SportID,
               GenderCategoryID = profileCoaches.GenderCategoryID,
               SchoolRegionID = profileCoaches.SchoolRegionID,
               SchoolDivisionID = profileCoaches.SchoolDivisionID,
               SchoolID = profileCoaches.SchoolID,
               SportCategoryID = profileCoaches.SportCategoryID
           };

        [HttpGet("Coach")] // /api/Profiles/Coach
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfileCoaches>>> GetProfileCoaches(
        [FromQuery] string? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? middleInitial = null,
        [FromQuery] string? sex = null,
        [FromQuery] DateTime? birthDate = null,
        [FromQuery] string? designation = null,
        [FromQuery] int? sportsID = null,
        [FromQuery] int? genderCategoryID = null,
        [FromQuery] int? sportCategoryID = null,
        [FromQuery] int? schoolRegionID = null,
        [FromQuery] int? schoolDivisionID = null,
        [FromQuery] int? schoolID = null)
        {
            var query = _context.ProfileCoaches.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName!.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName!.Contains(lastName));
            
            if (!string.IsNullOrEmpty(middleInitial))
                query = query.Where(x => x.MiddleInitial!.Contains(middleInitial));
            
            if (!string.IsNullOrEmpty(sex))
                query = query.Where(x => x.Sex!.Contains(sex));
            
            if (birthDate.HasValue)
                query = query.Where(x => x.BirthDate == birthDate.Value);
            
            if (!string.IsNullOrEmpty(designation))
                query = query.Where(x => x.Designation!.Contains(designation));
            
            if (sportCategoryID.HasValue)
                query = query.Where(x => x.SportCategoryID == sportCategoryID.Value);
            
            if (sportsID.HasValue)
                query = query.Where(x => x.SportID == sportsID.Value);
            
            if (genderCategoryID.HasValue)
                query = query.Where(x => x.GenderCategoryID == genderCategoryID.Value);

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);
            
            if (schoolDivisionID.HasValue)
                query = query.Where(x => x.SchoolDivisionID == schoolDivisionID.Value);
            
            if (schoolID.HasValue)
                query = query.Where(x => x.SchoolID == schoolID.Value);

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
                MiddleInitial = profileCoaches.MiddleInitial,
                Sex = profileCoaches.Sex,
                BirthDate = profileCoaches.BirthDate,
                Designation = profileCoaches.Designation,
                SportID = profileCoaches.SportID,
                GenderCategoryID = profileCoaches.GenderCategoryID,
                SchoolRegionID = profileCoaches.SchoolRegionID,
                SchoolDivisionID = profileCoaches.SchoolDivisionID,
                SchoolID = profileCoaches.SchoolID,
                SportCategoryID = profileCoaches.SportCategoryID
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
            existingCoachProfile.Sex = profileCoaches.Sex;
            existingCoachProfile.Designation = profileCoaches.Designation;
            existingCoachProfile.MiddleInitial = profileCoaches.MiddleInitial;
            existingCoachProfile.BirthDate = profileCoaches.BirthDate;
            existingCoachProfile.SportID = profileCoaches.SportID;
            existingCoachProfile.GenderCategoryID = profileCoaches.GenderCategoryID;
            existingCoachProfile.SchoolDivisionID = profileCoaches.SchoolDivisionID;
            existingCoachProfile.SchoolID = profileCoaches.SchoolID;
            existingCoachProfile.SportCategoryID = profileCoaches.SportCategoryID;

            
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
        public async Task<IActionResult> DeleteProfileCoaches(string id)
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
