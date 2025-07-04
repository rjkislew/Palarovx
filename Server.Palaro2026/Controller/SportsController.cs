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

        // ------------------------------------------------------------------------------------------------------------------

        // Sport Category Details view
        [HttpGet("Details")] // /api/Sports/Details
        public async Task<ActionResult<List<SportsDTO.SportDetails.SportCategory>>> SportCategoryDetails()
        {
            try
            {
                var sportCategories = await _context.SportCategories
                    .Include(c => c.Sports)!
                        .ThenInclude(s => s.SportSubcategories!)!
                            .ThenInclude(sc => sc.SchoolLevel)
                    .Include(c => c.Sports)!
                        .ThenInclude(s => s.SportSubcategories!)!
                            .ThenInclude(sc => sc.SportGenderCategory)
                    .AsNoTracking()
                    .ToListAsync();

                var result = sportCategories.Select(category => new SportsDTO.SportDetails.SportCategory
                {
                    ID = category.ID,
                    Category = category.Category,
                    SportsList = category.Sports?.Select(sport => new SportsDTO.SportDetails.Sports
                    {
                        ID = sport.ID,
                        Sport = sport.Sport,
                        Description = sport.Description,

                        SchoolLevelsList = sport.SportSubcategories?
                            .GroupBy(sub => new
                            {
                                ID = sub.SchoolLevel!.ID,
                                Level = sub.SchoolLevel?.Level
                            })
                            .Select(levelGroup => new SportsDTO.SportDetails.SchoolLevels
                            {
                                ID = levelGroup.Key.ID,
                                Level = levelGroup.Key.Level,
                                SportGenderCategoriesList = levelGroup
                                    .GroupBy(sub => new
                                    {
                                        ID = sub.SportGenderCategory!.ID ,
                                        Gender = sub.SportGenderCategory?.Gender
                                    })
                                    .Select(genderGroup => new SportsDTO.SportDetails.SportGenderCategories
                                    {
                                        ID = genderGroup.Key.ID,
                                        Gender = genderGroup.Key.Gender,
                                        SportSubcategoriesList = genderGroup
                                            .Select(sub => new SportsDTO.SportDetails.SportSubcategories
                                            {
                                                ID = sub.ID,
                                                Subcategory = sub.Subcategory
                                            }).ToList()
                                    }).ToList()
                            }).ToList()
                    }).ToList()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Sports REST methods

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

        // ------------------------------------------------------------------------------------------------------------------

        // Sport Categories REST methods

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

        // ------------------------------------------------------------------------------------------------------------------

        // Sport Gender Categories REST methods

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

        // ------------------------------------------------------------------------------------------------------------------

        // Sport Subcategories REST methods

        // Mapping for SportSubcategories entity to SportsDTO
        private static SportsDTO.SportSubcategories SportSubcategoriesDTOMapper(SportSubcategories sportSubcategories) =>
           new SportsDTO.SportSubcategories
           {
               ID = sportSubcategories.ID,
               Subcategory = sportSubcategories.Subcategory,
               SportID = sportSubcategories.SportID,
               SportGenderCategoryID = sportSubcategories.SportGenderCategoryID,
               SchoolLevelID = sportSubcategories.SchoolLevelID
           };

        [HttpGet("Subcategories")] // /api/Sports/Subcategories
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
                query = query.Where(x => x.Subcategory!.Contains(subcategory));

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

        [HttpPost("Subcategories")] // /api/Sports/Subcategories
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
