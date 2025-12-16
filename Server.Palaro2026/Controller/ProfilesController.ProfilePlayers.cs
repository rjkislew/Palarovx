using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class ProfilesController
    {// Map ProfilePlayers to ProfilesDTO.ProfilePlayers
        private static ProfilesDTO.ProfilePlayers ProfilePlayersDTOMapper(ProfilePlayers profilePlayers) =>
           new ProfilesDTO.ProfilePlayers
           {
               ID = profilePlayers.ID,
               FirstName = profilePlayers.FirstName,
               LastName = profilePlayers.LastName,
               SchoolID = profilePlayers.SchoolID,
               SportID = profilePlayers.SportID,
               MiddleInitial = profilePlayers.MiddleInitial,
               Sex = profilePlayers.Sex,
               BirthDate = profilePlayers.BirthDate,
               LRN = profilePlayers.LRN,
               SportCategoryID = profilePlayers.SportCategoryID,
           };

        [HttpGet("Player")] // /api/Profiles/Player
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayers>>> GetProfilePlayers(
        [FromQuery] string? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolID = null,
        [FromQuery] int? sportID = null,
        [FromQuery] string? middleInitial = null,
        [FromQuery] string? sex = null,
        [FromQuery] DateTime? birthDate = null,
        [FromQuery] string? lrn = null,
        [FromQuery] int? sportCategoryID = null)
        {
            var query = _context.ProfilePlayers.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName!.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName!.Contains(lastName));

            if (schoolID.HasValue)
                query = query.Where(x => x.SchoolID == schoolID.Value);
            
            if (sportCategoryID.HasValue)
                query = query.Where(x => x.SportCategoryID == sportCategoryID.Value);

            if (sportID.HasValue)
                query = query.Where(x => x.SportID == sportID.Value);
            
            if (!string.IsNullOrEmpty(middleInitial))
                query = query.Where(x => x.MiddleInitial!.Contains(middleInitial));
            
            if (!string.IsNullOrEmpty(sex))
                query = query.Where(x => x.Sex!.Contains(sex));
            
            if (birthDate.HasValue)
                query = query.Where(x => x.BirthDate == birthDate.Value);
            
            if (!string.IsNullOrEmpty(lrn))
                query = query.Where(p => p.LRN != null && p.LRN.Contains(lrn));
            
            return await query
                .Select(x => ProfilePlayersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Player")] // /api/Profiles/Player
        public async Task<ActionResult<ProfilePlayers>> PostProfilePlayers(ProfilesDTO.ProfilePlayers profilePlayers)
        {
            var profilePlayersDTO = new ProfilePlayers
            {
                ID = profilePlayers.ID,
                FirstName = profilePlayers.FirstName,
                LastName = profilePlayers.LastName,
                SchoolID = profilePlayers.SchoolID,
                SportID = profilePlayers.SportID,
                MiddleInitial = profilePlayers.MiddleInitial,
                Sex = profilePlayers.Sex,
                BirthDate = profilePlayers.BirthDate,
                LRN = profilePlayers.LRN,
                SportCategoryID = profilePlayers.SportCategoryID,
            };

            _context.ProfilePlayers.Add(profilePlayersDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayersExist(profilePlayers.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfilePlayers", new { id = profilePlayers.ID }, ProfilePlayersDTOMapper(profilePlayersDTO));
        }

        [HttpPut("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> PutProfilePlayers(string id, ProfilesDTO.ProfilePlayers profilePlayers)
        {
            if (id != profilePlayers.ID)
            {
                return BadRequest();
            }

            var existingPlayerProfile = await _context.ProfilePlayers.FindAsync(id);
            if (existingPlayerProfile == null)
            {
                return NotFound();
            }

            existingPlayerProfile.FirstName = profilePlayers.FirstName;
            existingPlayerProfile.LastName = profilePlayers.LastName;
            existingPlayerProfile.MiddleInitial = profilePlayers.MiddleInitial;
            existingPlayerProfile.Sex = profilePlayers.Sex;
            existingPlayerProfile.BirthDate = profilePlayers.BirthDate;
            existingPlayerProfile.LRN = profilePlayers.LRN;
            existingPlayerProfile.SchoolID = profilePlayers.SchoolID;
            existingPlayerProfile.SportID = profilePlayers.SportID;
            existingPlayerProfile.SportCategoryID = profilePlayers.SportCategoryID;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayersExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> PatchProfilePlayers(string id, [FromBody] ProfilesDTO.ProfilePlayers updatedPlayer)
        {
            var existingPlayer = await _context.ProfilePlayers.FindAsync(id);

            if (existingPlayer == null) return NotFound();

            if (updatedPlayer.FirstName != null) existingPlayer.FirstName = updatedPlayer.FirstName;
            if (updatedPlayer.LastName != null) existingPlayer.LastName = updatedPlayer.LastName;
            if (updatedPlayer.SchoolID != null) existingPlayer.SchoolID = updatedPlayer.SchoolID;
            if (updatedPlayer.SportID != null) existingPlayer.SportID = updatedPlayer.SportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayersExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayersExist(updatedPlayer.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> DeleteProfilePlayers(string id)
        {
            var ProfilePlayers = await _context.ProfilePlayers.FindAsync(id);
            if (ProfilePlayers == null)
            {
                return NotFound();
            }

            _context.ProfilePlayers.Remove(ProfilePlayers);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a ProfilePlayer exists by ID
        private bool ProfilePlayersExist(string id)
        {
            return _context.ProfilePlayers.Any(e => e.ID == id);
        }
    }
}
