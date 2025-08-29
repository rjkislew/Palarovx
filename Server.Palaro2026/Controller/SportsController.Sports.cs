using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class SportsController : ControllerBase
    {
        // Mapping for Sports entity to sportsDTO
        private static SportsDTO.Sports SportsDTOMapper(Sports sports) =>
           new SportsDTO.Sports
           {
               ID = sports.ID,
               Sport = sports.Sport,
               Description = sports.Description,
               SportCategoryID = sports.SportCategoryID,
           };

        [HttpGet] // /api/Sports
        public async Task<ActionResult<IEnumerable<SportsDTO.Sports>>> GetSports(
        [FromQuery] string? id = null,
        [FromQuery] string? sport = null,
        [FromQuery] string? description = null,
        [FromQuery] int? sportCategoryID = null)
        {
            var query = _context.Sports.AsQueryable();

            if (!string.IsNullOrWhiteSpace(id))
            {
                var idList = id.Split(',')
                               .Select(s => int.TryParse(s, out int val) ? val : (int?)null)
                               .Where(val => val.HasValue)
                               .Select(val => val!.Value)
                               .ToList();

                if (idList.Any())
                    query = query.Where(x => idList.Contains(x.ID));
            }

            if (!string.IsNullOrEmpty(sport))
                query = query.Where(x => x.Sport!.Contains(sport));

            if (!string.IsNullOrEmpty(description))
                query = query.Where(x => x.Description!.Contains(description));

            if (sportCategoryID.HasValue)
                query = query.Where(x => x.SportCategoryID == sportCategoryID.Value);

            return await query
                .Select(x => SportsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost] // /api/Sports
        public async Task<ActionResult<Sports>> PostSports(SportsDTO.Sports sports)
        {
            var sportsDTO = new Sports
            {
                ID = sports.ID,
                Sport = sports.Sport,
                Description = sports.Description,
                SportCategoryID = sports.SportCategoryID,
            };

            _context.Sports.Add(sportsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SportsExist(sports.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSports", new { id = sports.ID }, SportsDTOMapper(sportsDTO));
        }

        [HttpPut("{id}")] // /api/Sports/{id}
        public async Task<IActionResult> PutSports(int id, SportsDTO.Sports sports)
        {
            if (id != sports.ID)
            {
                return BadRequest();
            }

            var existingSport = await _context.Sports.FindAsync(id);
            if (existingSport == null)
            {
                return NotFound();
            }

            existingSport.Sport = sports.Sport;
            existingSport.Description = sports.Description;
            existingSport.SportCategoryID = sports.SportCategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportsExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("{id}")] // /api/Sports/{id}
        public async Task<IActionResult> PatchSports(int id, [FromBody] SportsDTO.Sports updatedSport)
        {
            var existingSport = await _context.Sports.FindAsync(id);
            if (existingSport == null) return NotFound();

            if (updatedSport.Sport != null) existingSport.Sport = updatedSport.Sport;
            if (updatedSport.Description != null) existingSport.Description = updatedSport.Description;
            if (updatedSport.SportCategoryID != null) existingSport.SportCategoryID = updatedSport.SportCategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Sports.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")] // /api/Sports/{id}
        public async Task<IActionResult> DeleteSports(int id)
        {
            var sports = await _context.Sports.FindAsync(id);
            if (sports == null)
            {
                return NotFound();
            }

            _context.Sports.Remove(sports);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a sport exists by ID
        private bool SportsExist(int id)
        {
            return _context.Sports.Any(e => e.ID == id);
        }
    }
}
