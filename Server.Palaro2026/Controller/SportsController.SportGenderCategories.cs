using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class SportsController : ControllerBase
    {
        // Mapping for SportGenderCategories entity to SportsDTO
        private static SportsDTO.SportGenderCategories SportGenderCategoriesDTOMapper(SportGenderCategories sportGenderCategories) =>
           new SportsDTO.SportGenderCategories
           {
               ID = sportGenderCategories.ID,
               Gender = sportGenderCategories.Gender
           };

        [HttpGet("GenderCategories")] // /api/Sports/GenderCategories
        public async Task<ActionResult<IEnumerable<SportsDTO.SportGenderCategories>>> GetSportGenderCategories(
        [FromQuery] int? id = null,
        [FromQuery] string? gender = null)
        {
            var query = _context.SportGenderCategories.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(x => x.Gender!.Contains(gender));

            return await query
                .Select(x => SportGenderCategoriesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("GenderCategories")] // /api/Sports/GenderCategories
        public async Task<ActionResult<SportGenderCategories>> PostSportGenderCategories(SportsDTO.SportGenderCategories sportGenderCategories)
        {
            var sportGenderCategoriesDTO = new SportGenderCategories
            {
                ID = sportGenderCategories.ID,
                Gender = sportGenderCategories.Gender
            };
            _context.SportGenderCategories.Add(sportGenderCategoriesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SportGenderCategoriesExist(sportGenderCategories.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSportGenderCategories", new { id = sportGenderCategories.ID }, SportGenderCategoriesDTOMapper(sportGenderCategoriesDTO));
        }

        [HttpPut("GenderCategories/{id}")] // /api/Sports/GenderCategories/{id}
        public async Task<IActionResult> PutSportGenderCategories(int id, SportsDTO.SportGenderCategories sportGenderCategories)
        {
            if (id != sportGenderCategories.ID)
            {
                return BadRequest();
            }

            var existingGenderCategory = await _context.SportGenderCategories.FindAsync(id);
            if (existingGenderCategory == null)
            {
                return NotFound();
            }

            existingGenderCategory.Gender = sportGenderCategories.Gender;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportGenderCategoriesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("GenderCategories/{id}")] // /api/Sports/GenderCategories/{id}
        public async Task<IActionResult> PatchSportGenderCategories(int id, [FromBody] SportsDTO.SportGenderCategories updatedGenderCategory)
        {
            var existingGenderCategory = await _context.SportGenderCategories.FindAsync(id);
            if (existingGenderCategory == null) return NotFound();

            if (updatedGenderCategory.Gender != null)
                existingGenderCategory.Gender = updatedGenderCategory.Gender;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SportGenderCategories.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("GenderCategories/{id}")] // /api/Sports/GenderCategories/{id}
        public async Task<IActionResult> DeleteSportGenderCategories(int id)
        {
            var sportGenderCategories = await _context.SportGenderCategories.FindAsync(id);
            if (sportGenderCategories == null)
            {
                return NotFound();
            }

            _context.SportGenderCategories.Remove(sportGenderCategories);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a SportGenderCategory exists by ID
        private bool SportGenderCategoriesExist(int id)
        {
            return _context.SportGenderCategories.Any(e => e.ID == id);
        }
    }
}
