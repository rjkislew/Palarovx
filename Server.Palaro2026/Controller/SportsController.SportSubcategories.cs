using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class SportsController : ControllerBase
    {
        // Mapping for SportSubcategories entity to SportsDTO
        private static SportsDTO.SportSubcategories SportSubcategoriesDTOMapper(SportSubcategories sportSubcategories) =>
           new SportsDTO.SportSubcategories
           {
               ID = sportSubcategories.ID,
               Subcategory = sportSubcategories.Subcategory,
               SportID = sportSubcategories.SportID,
               SportGenderCategoryID = sportSubcategories.SportGenderCategoryID,
               SchoolLevelID = sportSubcategories.SchoolLevelID,
               MainCategory = sportSubcategories.MainCategory
           };

        [HttpGet("Subcategories")] // /api/Sports/Subcategories
        public async Task<ActionResult<IEnumerable<SportsDTO.SportSubcategories>>> GetSportSubcategories(
        [FromQuery] int? ID = null,
        [FromQuery] string? subcategory = null,
        [FromQuery] int? sportID = null,
        [FromQuery] int? sportGenderCategoryID = null,
        [FromQuery] int? schoolLevelID = null,
        [FromQuery] string? maincategory = null)
        {
            var query = _context.SportSubcategories.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(subcategory))
                query = query.Where(x => x.Subcategory!.Contains(subcategory));

            if (sportID.HasValue)
                query = query.Where(x => x.SportID == sportID.Value);

            if (sportGenderCategoryID.HasValue)
                query = query.Where(x => x.SportGenderCategoryID == sportGenderCategoryID.Value);

            if (schoolLevelID.HasValue)
                query = query.Where(x => x.SchoolLevelID == schoolLevelID.Value);

            if (!string.IsNullOrEmpty(maincategory))
                query = query.Where(x => x.MainCategory!.Contains(maincategory));

            return await query
                .Select(x => SportSubcategoriesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Subcategories")] // /api/Sports/Subcategories
        public async Task<ActionResult<SportSubcategories>> PostSportSubcategories(SportsDTO.SportSubcategories sportSubcategories)
        {
            var sportSubcategoriesDTO = new SportSubcategories
            {
                ID = sportSubcategories.ID,
                Subcategory = sportSubcategories.Subcategory,
                SportID = sportSubcategories.SportID,
                SportGenderCategoryID = sportSubcategories.SportGenderCategoryID,
                SchoolLevelID = sportSubcategories.SchoolLevelID,
                MainCategory = sportSubcategories.MainCategory
            };
            _context.SportSubcategories.Add(sportSubcategoriesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SportSubcategoriesExist(sportSubcategories.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSportSubcategories", new { id = sportSubcategories.ID }, SportSubcategoriesDTOMapper(sportSubcategoriesDTO));
        }

        [HttpPut("Subcategories/{id}")] // /api/Sports/Subcategories/{id}
        public async Task<IActionResult> PutSportSubcategories(int id, SportsDTO.SportSubcategories sportSubcategories)
        {
            if (id != sportSubcategories.ID)
            {
                return BadRequest();
            }

            var existingSubcategory = await _context.SportSubcategories.FindAsync(id);
            if (existingSubcategory == null)
            {
                return NotFound();
            }

            existingSubcategory.Subcategory = sportSubcategories.Subcategory;
            existingSubcategory.SportID = sportSubcategories.SportID;
            existingSubcategory.SportGenderCategoryID = sportSubcategories.SportGenderCategoryID;
            existingSubcategory.SchoolLevelID = sportSubcategories.SchoolLevelID;
            existingSubcategory.MainCategory = sportSubcategories.MainCategory;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportSubcategoriesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Subcategories/{id}")] // /api/Sports/Subcategories/{id}
        public async Task<IActionResult> PatchSportSubcategories(int id, [FromBody] SportsDTO.SportSubcategories updatedSubcategory)
        {
            var existingSubcategory = await _context.SportSubcategories.FindAsync(id);
            if (existingSubcategory == null) return NotFound();

            if (updatedSubcategory.Subcategory != null)
                existingSubcategory.Subcategory = updatedSubcategory.Subcategory;

            if (updatedSubcategory.SportID != null)
                existingSubcategory.SportID = updatedSubcategory.SportID;

            if (updatedSubcategory.SportGenderCategoryID != null)
                existingSubcategory.SportGenderCategoryID = updatedSubcategory.SportGenderCategoryID;

            if (updatedSubcategory.SchoolLevelID != null)
                existingSubcategory.SchoolLevelID = updatedSubcategory.SchoolLevelID;

            if (updatedSubcategory.MainCategory != null)
                existingSubcategory.MainCategory = updatedSubcategory.MainCategory;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SportSubcategories.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Subcategories/{id}")] // /api/Sports/Subcategories/{id}
        public async Task<IActionResult> DeleteSportSubcategories(int id)
        {
            var sportSubcategories = await _context.SportSubcategories.FindAsync(id);
            if (sportSubcategories == null)
            {
                return NotFound();
            }

            _context.SportSubcategories.Remove(sportSubcategories);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a SportSubcategory exists by ID
        private bool SportSubcategoriesExist(int id)
        {
            return _context.SportSubcategories.Any(e => e.ID == id);
        }
    }
}
