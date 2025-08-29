using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class NewsController : ControllerBase
    {
        // mapping NewsCategories to NewsCategoriesDTO
        private static NewsCategories NewsCategoryDTOMapper(NewsCategories category) => new NewsCategories
        {
            ID = category.ID,
            Category = category.Category,
            Description = category.Description
        };

        [HttpGet("Categories")] // /api/News/Categories
        public async Task<ActionResult<IEnumerable<NewsCategories>>> GetNewsCategories(
            [FromQuery] int? id = null,
            [FromQuery] string? category = null)
        {
            var query = _context.NewsCategories.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category!.Contains(category));

            return await query
                .Select(x => NewsCategoryDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Categories")] // /api/News/Categories
        public async Task<ActionResult<NewsCategories>> PostNewsCategory(NewsCategories newsCategory)
        {
            var newsCategoriesDTO = new NewsCategories
            {
                ID = newsCategory.ID,
                Category = newsCategory.Category,
                Description = newsCategory.Description
            };

            _context.NewsCategories.Add(newsCategoriesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (NewsCategoryExist(newsCategory.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetNewsCategories", new { id = newsCategory.ID }, NewsCategoryDTOMapper(newsCategoriesDTO));
        }

        [HttpPut("Categories/{id}")] // /api/News/Categories/{id}
        public async Task<IActionResult> PutNewsCategory(int id, NewsCategories category)
        {
            if (id != category.ID)
                return BadRequest();

            var existing = await _context.NewsCategories.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Category = category.Category;

            existing.Description = category.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsCategoryExist(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpPatch("Categories/{id}")] // /api/News/Categories/{id}
        public async Task<IActionResult> PatchNewsCategory(int id, [FromBody] NewsCategories category)
        {
            var existing = await _context.NewsCategories.FindAsync(id);
            if (existing == null)
                return NotFound();

            if (!string.IsNullOrEmpty(category.Category))
                existing.Category = category.Category;

            existing.Description = category.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsCategoryExist(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("Categories/{id}")] // /api/News/Categories/{id}
        public async Task<IActionResult> DeleteNewsCategory(int id)
        {
            var category = await _context.NewsCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.NewsCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper Method
        private bool NewsCategoryExist(int id)
        {
            return _context.NewsCategories.Any(x => x.ID == id);
        }
    }
}
