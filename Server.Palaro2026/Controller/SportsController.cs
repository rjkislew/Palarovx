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
        public async Task<ActionResult<IEnumerable<SportsDTO.Sports>>> GetSports()
        {
            return await _context.Sports
                .Select(x => SportsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SportsDTO.Sports>> GetSports(int id)
        {
            var sports = await _context.Sports.FindAsync(id);

            if (sports == null)
            {
                return NotFound();
            }

            return SportsDTOMapper(sports);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSports(int id, SportsDTO.Sports sports)
        {
            if (id != sports.ID)
            {
                return BadRequest();
            }

            _context.Entry(sports).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
        public async Task<ActionResult<IEnumerable<SportsDTO.SportCategories>>> GetSportCategories()
        {
            return await _context.SportCategories
                .Select(x => SportCategoriesDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Categories/{id}")]
        public async Task<ActionResult<SportsDTO.SportCategories>> GetSportCategories(int id)
        {
            var sportCategories = await _context.SportCategories.FindAsync(id);

            if (sportCategories == null)
            {
                return NotFound();
            }

            return SportCategoriesDTOMapper(sportCategories);
        }

        [HttpPut("Categories/{id}")]
        public async Task<IActionResult> PutSportCategories(int id, SportsDTO.SportCategories sportCategories)
        {
            if (id != sportCategories.ID)
            {
                return BadRequest();
            }

            _context.Entry(sportCategories).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
        public async Task<ActionResult<IEnumerable<SportsDTO.SportGenderCategories>>> GetSportGenderCategories()
        {
            return await _context.SportGenderCategories
                .Select(x => SportGenderCategoriesDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("GenderCategories/{id}")]
        public async Task<ActionResult<SportsDTO.SportGenderCategories>> GetSportGenderCategories(int id)
        {
            var sportGenderCategories = await _context.SportGenderCategories.FindAsync(id);

            if (sportGenderCategories == null)
            {
                return NotFound();
            }

            return SportGenderCategoriesDTOMapper(sportGenderCategories);
        }

        [HttpPut("GenderCategories/{id}")]
        public async Task<IActionResult> PutSportGenderCategories(int id, SportsDTO.SportGenderCategories sportGenderCategories)
        {
            if (id != sportGenderCategories.ID)
            {
                return BadRequest();
            }

            _context.Entry(sportGenderCategories).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
        public async Task<ActionResult<IEnumerable<SportsDTO.SportSubcategories>>> GetSportSubcategories()
        {
            return await _context.SportSubcategories
                .Select(x => SportSubcategoriesDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Subcategories/{id}")]
        public async Task<ActionResult<SportsDTO.SportSubcategories>> GetSportSubcategories(int id)
        {
            var sportSubcategories = await _context.SportSubcategories.FindAsync(id);

            if (sportSubcategories == null)
            {
                return NotFound();
            }

            return SportSubcategoriesDTOMapper(sportSubcategories);
        }

        [HttpPut("Subcategories/{id}")]
        public async Task<IActionResult> PutSportSubcategories(int id, SportsDTO.SportSubcategories sportSubcategories)
        {
            if (id != sportSubcategories.ID)
            {
                return BadRequest();
            }

            _context.Entry(sportSubcategories).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
