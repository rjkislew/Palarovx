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
        // Map ProfilePlayerSports to ProfilesDTO.ProfilePlayerSports
        private static ProfilesDTO.ProfilePlayerSports ProfilePlayerSportsDTOMapper(ProfilePlayerSports profilePlayerSports) =>
           new ProfilesDTO.ProfilePlayerSports
           {
               ID = profilePlayerSports.ID,
               ProfilePlayerID = profilePlayerSports.ProfilePlayerID,
               SportSubcategoryID = profilePlayerSports.SportSubcategoryID,
           };

        [HttpGet("Player/Sports")] // /api/Profiles/Player/Sports
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayerSports>>> GetProfilePlayerSports(
        [FromQuery] int? ID = null,
        [FromQuery] string? profilePlayerID = null,
        [FromQuery] int? sportSubcategoryID = null)
        {
            var query = _context.ProfilePlayerSports.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(profilePlayerID))
                query = query.Where(x => x.ProfilePlayerID == profilePlayerID);

            if (sportSubcategoryID.HasValue)
                query = query.Where(x => x.SportSubcategoryID == sportSubcategoryID.Value);

            return await query
                .AsNoTracking()
                .Select(x => ProfilePlayerSportsDTOMapper(x))
                .ToListAsync();
        }

        [HttpPost("Player/Sports")] // /api/Profiles/Player/Sports
        public async Task<ActionResult<ProfilePlayerSports>> PostProfilePlayerSports(ProfilesDTO.ProfilePlayerSports profilePlayerSports)
        {
            var profilePlayerSportsDTO = new ProfilePlayerSports
            {
                ID = profilePlayerSports.ID,
                ProfilePlayerID = profilePlayerSports.ProfilePlayerID,
                SportSubcategoryID = profilePlayerSports.SportSubcategoryID,
            };

            _context.ProfilePlayerSports.Add(profilePlayerSportsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayerSportsExist(profilePlayerSports.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfilePlayerSports", new { id = profilePlayerSports.ID }, ProfilePlayerSportsDTOMapper(profilePlayerSportsDTO));
        }

        [HttpPut("Player/Sports/{id}")] // /api/Profiles/Player/Sports/{id}
        public async Task<IActionResult> PutProfilePlayerSports(int id, ProfilesDTO.ProfilePlayerSports profilePlayerSports)
        {
            if (id != profilePlayerSports.ID)
            {
                return BadRequest();
            }

            var existingProfilePlayerSport = await _context.ProfilePlayerSports.FindAsync(id);
            if (existingProfilePlayerSport == null)
            {
                return NotFound();
            }

            existingProfilePlayerSport.ProfilePlayerID = profilePlayerSports.ProfilePlayerID;
            existingProfilePlayerSport.SportSubcategoryID = profilePlayerSports.SportSubcategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayerSportsExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Player/Sports/{id}")] // /api/Profiles/Player/Sports/{id}
        public async Task<IActionResult> PatchProfilePlayerSports(int id, [FromBody] ProfilesDTO.ProfilePlayerSports updatedSport)
        {
            var existingSport = await _context.ProfilePlayerSports.FindAsync(id);

            if (existingSport == null) return NotFound();

            if (updatedSport.ProfilePlayerID != null) existingSport.ProfilePlayerID = updatedSport.ProfilePlayerID;
            if (updatedSport.SportSubcategoryID != null) existingSport.SportSubcategoryID = updatedSport.SportSubcategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayerSportsExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayerSportsExist(updatedSport.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Player/Sports/{id}")] // /api/Profiles/Player/Sports/{id}
        public async Task<IActionResult> DeleteProfilePlayerSports(int id)
        {
            var profilePlayerSports = await _context.ProfilePlayerSports.FindAsync(id);
            if (profilePlayerSports == null)
            {
                return NotFound();
            }

            _context.ProfilePlayerSports.Remove(profilePlayerSports);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Delete ProfilePlayerSports by PlayerSportID
        private bool ProfilePlayerSportsExist(int id)
        {
            return _context.ProfilePlayerSports.Any(e => e.ID == id);
        }
    }
}
