using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class SportsController : ControllerBase
    {
        // Mapping for SportCategories entity to SportsDTO
        private static SportsDTO.SportCategories SportCategoriesDTOMapper(SportCategories sportCategories) =>
           new SportsDTO.SportCategories
           {
               ID = sportCategories.ID,
               Category = sportCategories.Category
           };

        [HttpGet("Categories")] // /api/Sports/Categories
        public async Task<ActionResult<IEnumerable<SportsDTO.SportCategories>>> GetSportCategories(
        [FromQuery] int? id = null,
        [FromQuery] string? category = null)
        {
            var query = _context.SportCategories.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category!.Contains(category));

            return await query
                .Select(x => SportCategoriesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Categories")] // /api/Sports/Categories
        public async Task<ActionResult<SportCategories>> PostSportCategories(SportsDTO.SportCategories sportCategories)
        {
            var sportCategoriesDTO = new SportCategories
            {
                ID = sportCategories.ID,
                Category = sportCategories.Category
            };

            _context.SportCategories.Add(sportCategoriesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SportCategoriesExist(sportCategories.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSportCategories", new { id = sportCategories.ID }, SportCategoriesDTOMapper(sportCategoriesDTO));
        }

        [HttpPut("Categories/{id}")] // /api/Sports/Categories/{id}
        public async Task<IActionResult> PutSportCategories(int id, SportsDTO.SportCategories sportCategories)
        {
            if (id != sportCategories.ID)
            {
                return BadRequest();
            }

            var existingCategory = await _context.SportCategories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Category = sportCategories.Category;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportCategoriesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Categories/{id}")] // /api/Sports/Categories/{id}
        public async Task<IActionResult> PatchSportCategories(int id, [FromBody] SportsDTO.SportCategories updatedCategory)
        {
            var existingCategory = await _context.SportCategories.FindAsync(id);
            if (existingCategory == null) return NotFound();

            if (updatedCategory.Category != null)
                existingCategory.Category = updatedCategory.Category;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SportCategories.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Categories/{id}")] // /api/Sports/Categories/{id}
        public async Task<IActionResult> DeleteSportCategories(int id)
        {
            var sportCategories = await _context.SportCategories.FindAsync(id);
            if (sportCategories == null)
            {
                return NotFound();
            }

            _context.SportCategories.Remove(sportCategories);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a sport category exists by ID
        private bool SportCategoriesExist(int id)
        {
            return _context.SportCategories.Any(e => e.ID == id);
        }
    }
}
