using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public SportsController(Palaro2026Context context)
        {
            _context = context;
        }

        private static SportsDTO.Sports SportsDTOMapper(Sports sports) =>
           new SportsDTO.Sports
           {
               ID = sports.ID,
               Sport = sports.Sport,
               Description = sports.Description,
               SportCategoryID = sports.SportCategoryID,
           };

        [HttpGet]
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
                query = query.Where(x => x.Sport.Contains(sport));

            if (!string.IsNullOrEmpty(description))
                query = query.Where(x => x.Description.Contains(description));

            if (sportCategoryID.HasValue)
                query = query.Where(x => x.SportCategoryID == sportCategoryID.Value);

            return await query
                .Select(x => SportsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }


        [HttpPut("{id}")]
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
                if (!SportsExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost]
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
                if (SportsExists(sports.ID))
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

        [HttpDelete("{id}")]
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

        private bool SportsExists(int id)
        {
            return _context.Sports.Any(e => e.ID == id);
        }




        // Sport Categories
        private static SportsDTO.SportCategories SportCategoriesDTOMapper(SportCategories sportCategories) =>
           new SportsDTO.SportCategories
           {
               ID = sportCategories.ID,
               Category = sportCategories.Category
           };

        [HttpGet("Categories")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportCategories>>> GetSportCategories(
        [FromQuery] int? id = null,
        [FromQuery] string? category = null)
        {
            var query = _context.SportCategories.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category.Contains(category));

            return await query
                .Select(x => SportCategoriesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Categories/{id}")]
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
                if (!SportCategoriesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Categories")]
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
                if (SportCategoriesExists(sportCategories.ID))
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

        [HttpDelete("Categories/{id}")]
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
        private bool SportCategoriesExists(int id)
        {
            return _context.SportCategories.Any(e => e.ID == id);
        }




        // Sport Gender Categories

        private static SportsDTO.SportGenderCategories SportGenderCategoriesDTOMapper(SportGenderCategories sportGenderCategories) =>
           new SportsDTO.SportGenderCategories
           {
               ID = sportGenderCategories.ID,
               Gender = sportGenderCategories.Gender
           };

        [HttpGet("GenderCategories")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportGenderCategories>>> GetSportGenderCategories(
        [FromQuery] int? id = null,
        [FromQuery] string? gender = null)
        {
            var query = _context.SportGenderCategories.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(x => x.Gender.Contains(gender));

            return await query
                .Select(x => SportGenderCategoriesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("GenderCategories/{id}")]
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
                if (!SportGenderCategoriesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("GenderCategories")]
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
                if (SportGenderCategoriesExists(sportGenderCategories.ID))
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

        [HttpDelete("GenderCategories/{id}")]
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

        private bool SportGenderCategoriesExists(int id)
        {
            return _context.SportGenderCategories.Any(e => e.ID == id);
        }




        // Sport Subcategories

        private static SportsDTO.SportSubcategories SportSubcategoriesDTOMapper(SportSubcategories sportSubcategories) =>
           new SportsDTO.SportSubcategories
           {
               ID = sportSubcategories.ID,
               Subcategory = sportSubcategories.Subcategory,
               SportID = sportSubcategories.SportID,
               SportGenderCategoryID = sportSubcategories.SportGenderCategoryID,
               SchoolLevelID = sportSubcategories.SchoolLevelID
           };

        [HttpGet("Subcategories")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportSubcategories>>> GetSportSubcategories(
        [FromQuery] int? ID = null,
        [FromQuery] string? subcategory = null,
        [FromQuery] int? sportID = null,
        [FromQuery] int? sportGenderCategoryID = null,
        [FromQuery] int? schoolLevelID = null)
        {
            var query = _context.SportSubcategories.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(subcategory))
                query = query.Where(x => x.Subcategory.Contains(subcategory));

            if (sportID.HasValue)
                query = query.Where(x => x.SportID == sportID.Value);

            if (sportGenderCategoryID.HasValue)
                query = query.Where(x => x.SportGenderCategoryID == sportGenderCategoryID.Value);

            if (schoolLevelID.HasValue)
                query = query.Where(x => x.SchoolLevelID == schoolLevelID.Value);

            return await query
                .Select(x => SportSubcategoriesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Subcategories/{id}")]
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportSubcategoriesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Subcategories")]
        public async Task<ActionResult<SportSubcategories>> PostSportSubcategories(SportsDTO.SportSubcategories sportSubcategories)
        {
            var sportSubcategoriesDTO = new SportSubcategories
            {
                ID = sportSubcategories.ID,
                Subcategory = sportSubcategories.Subcategory,
                SportID = sportSubcategories.SportID,
                SportGenderCategoryID = sportSubcategories.SportGenderCategoryID,
                SchoolLevelID = sportSubcategories.SchoolLevelID
            };
            _context.SportSubcategories.Add(sportSubcategoriesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SportSubcategoriesExists(sportSubcategories.ID))
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

        [HttpDelete("Subcategories/{id}")]
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

        private bool SportSubcategoriesExists(int id)
        {
            return _context.SportSubcategories.Any(e => e.ID == id);
        }
    }
}
