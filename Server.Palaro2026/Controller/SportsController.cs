using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        // Get all categories
        [HttpGet("Categories")]
        public async Task<ActionResult<IEnumerable<SportsDTO.s_CategoriesDTO>>> GetCategories()
        {
            var categories = await _context.SportCategories
                .Select(c => new SportsDTO.s_CategoriesDTO { Category = c.Category })
                .ToListAsync();

            return Ok(categories);
        }

        // Get Levels for a specific Category using POST
        [HttpGet("Levels")]
        public async Task<ActionResult<IEnumerable<SportsDTO.s_LevelsDTO>>> GetLevels([FromQuery] string Category)
        {
            var Levels = await (from sc in _context.SportCategories
                                join s in _context.Sports on sc.ID equals s.SportsCategoryID
                                join sub in _context.SportSubCategories on s.ID equals sub.SportID
                                join l in _context.Levels on sub.LevelID equals l.ID
                                where sc.Category == Category
                                select new SportsDTO.s_LevelsDTO
                                {
                                    ID = l.ID,
                                    Level = l.Level
                                })
                                .Distinct().ToListAsync();

            return Ok(Levels);
        }

        // Get Sports for a specific Category and Level using POST
        [HttpGet("Sports")]
        public async Task<ActionResult<IEnumerable<SportsDTO.s_SportsDTO>>> GetSports([FromQuery] string Category, [FromQuery] string Level)
        {
            var Sports = await (from sc in _context.SportCategories
                                join s in _context.Sports on sc.ID equals s.SportsCategoryID
                                join sub in _context.SportSubCategories on s.ID equals sub.SportID
                                join l in _context.Levels on sub.LevelID equals l.ID
                                where sc.Category == Category && l.Level == Level
                                select new SportsDTO.s_SportsDTO
                                {
                                    ID = s.ID,
                                    Sport = s.Sport,
                                    Description = s.Description
                                }).Distinct().ToListAsync();

            return Ok(Sports);
        }

        // Get genders for a specific Category, Level, and Sport using POST
        [HttpGet("Genders")]
        public async Task<ActionResult<IEnumerable<SportsDTO.s_GendersDTO>>> GetGenders([FromQuery] string Category, [FromQuery] string Level, [FromQuery] string Sport)
        {
            var genders = await (from sc in _context.SportCategories
                                 join s in _context.Sports on sc.ID equals s.SportsCategoryID
                                 join sub in _context.SportSubCategories on s.ID equals sub.SportID
                                 join gc in _context.GenderCategories on sub.GenderCategoryID equals gc.ID
                                 join l in _context.Levels on sub.LevelID equals l.ID
                                 where sc.Category == Category && l.Level == Level && s.Sport == Sport
                                 select new SportsDTO.s_GendersDTO
                                 {
                                     ID = gc.ID,
                                     GenderCategory = gc.GenderCategory
                                 })
                                 .Distinct().ToListAsync();

            return Ok(genders);
        }

        // Get sub-categories for a specific Category, Level, Sport, and gender using POST
        [HttpGet("SubCategories")]
        public async Task<ActionResult<IEnumerable<SportsDTO.s_SubCategoriesDTO>>> GetSubCategories([FromQuery] string Category, [FromQuery] string Level, [FromQuery] string Sport, [FromQuery] string gender)
        {
            var subCategories = await (from sc in _context.SportCategories
                                       join s in _context.Sports on sc.ID equals s.SportsCategoryID
                                       join sub in _context.SportSubCategories on s.ID equals sub.SportID
                                       join gc in _context.GenderCategories on sub.GenderCategoryID equals gc.ID
                                       join l in _context.Levels on sub.LevelID equals l.ID
                                       where sc.Category == Category && l.Level == Level && s.Sport == Sport && gc.GenderCategory == gender
                                       select new SportsDTO.s_SubCategoriesDTO
                                       {
                                           ID = sub.ID,
                                           SubCategory = sub.SubCategory
                                       })
                                       .Distinct().ToListAsync();

            return Ok(subCategories);
        }

        [HttpGet("SportLevelsGendersSubCategories")]
        public async Task<ActionResult<IEnumerable<SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_CategoriesDTO>>> GetSportsDTO()
        {
            var sportsData = await (from sports_categories in _context.SportCategories
                                    join Sports in _context.Sports on sports_categories.ID equals Sports.SportsCategoryID into sportGroup
                                    from Sport in sportGroup.DefaultIfEmpty()
                                    join SportSubCategories in _context.SportSubCategories on Sport.ID equals SportSubCategories.SportID into subGroup
                                    from sub in subGroup.DefaultIfEmpty()
                                    join Levels in _context.Levels on sub.LevelID equals Levels.ID into levelGroup
                                    from Level in levelGroup.DefaultIfEmpty()
                                    join GenderCategories in _context.GenderCategories on sub.GenderCategoryID equals GenderCategories.ID into genderGroup
                                    from gender in genderGroup.DefaultIfEmpty()
                                    select new
                                    {
                                        Category = sports_categories != null ? sports_categories.Category : null,
                                        Level = Level != null ? Level.Level : null,
                                        GenderCategory = gender != null ? gender.GenderCategory : null,
                                        Sport = Sport != null ? Sport.Sport : null,
                                        Description = Sport != null ? Sport.Description : null,
                                        SubCategory = sub != null ? sub.SubCategory : null
                                    }).ToListAsync();

            // Grouping logic to structure the data
            var groupedData = sportsData
                .GroupBy(x => x.Category)
                .Select(g => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_CategoriesDTO
                {
                    Category = g.Key,
                    Levels = g.GroupBy(x => x.Level)
                              .Select(l => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_LevelsDTO
                              {
                                  Level = l.Key,
                                  Sports = l.GroupBy(x => x.Sport)
                                           .Select(sg => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_SportsDTO
                                           {
                                               Sport = sg.Key,
                                               Description = sg.First().Description, // Use First() to get the Description from grouped data
                                               GenderCategories = sg.GroupBy(x => x.GenderCategory)
                                                                     .Select(gc => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_GendersDTO
                                                                     {
                                                                         GenderCategory = gc.Key,
                                                                         sub_categories = gc.Select(s => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_SubCategoriesDTO
                                                                         {
                                                                             SubCategory = s.SubCategory
                                                                         }).Distinct().ToList()
                                                                     }).ToList()
                                           }).ToList()
                              }).ToList()
                }).ToList();

            return Ok(groupedData);
        }


        [HttpGet("SportCategories")]
        public async Task<ActionResult<IEnumerable<SportCategoriesDTO.sc_CategoriesDTO>>> GetSportCategories()
        {
            var categoriesData = await (from sports_categories in _context.SportCategories
                                        join Sports in _context.Sports on sports_categories.ID equals Sports.SportsCategoryID into sportGroup
                                        from Sport in sportGroup.DefaultIfEmpty()
                                        select new
                                        {
                                            sports_categories.Category,
                                            Sport = Sport != null ? Sport.Sport : null,
                                            Description = Sport != null ? Sport.Description : null
                                        }).ToListAsync();

            // Grouping logic to structure the data
            var groupedData = categoriesData
                .GroupBy(x => x.Category)
                .Select(g => new SportCategoriesDTO.sc_CategoriesDTO
                {
                    Category = g.Key,
                    Sports = g.Where(x => x.Sport != null)
                              .Select(s => new SportCategoriesDTO.sc_SportsDTO
                              {
                                  Sport = s.Sport,
                                  Description = s.Description
                              }).ToList()
                }).ToList();

            return Ok(groupedData);
        }

        [HttpGet("SportSubCategories")]
        public async Task<ActionResult<IEnumerable<SportCategoriesSubCategoriesDTO.scsc_CategoriesDTO>>> GetSportSubCategories()
        {
            var sportsData = await (from sports_categories in _context.SportCategories
                                    join Sports in _context.Sports on sports_categories.ID equals Sports.SportsCategoryID into sportGroup
                                    from Sport in sportGroup.DefaultIfEmpty()
                                    join SportSubCategories in _context.SportSubCategories on Sport.ID equals SportSubCategories.SportID into subGroup
                                    from sub in subGroup.DefaultIfEmpty()
                                    select new
                                    {
                                        sports_categories.Category,
                                        Sport = Sport != null ? Sport.Sport : null,
                                        Description = Sport != null ? Sport.Description : null,
                                        SubCategory = sub != null ? sub.SubCategory : null
                                    }).ToListAsync();

            // Grouping logic to structure the data
            var groupedData = sportsData
                .OrderByDescending(c => c.Category)
                .GroupBy(c => c.Category)
                .Select(s => new SportCategoriesSubCategoriesDTO.scsc_CategoriesDTO
                {
                    Category = s.Key,
                    Sports = s.Where(s => s.Sport != null)
                              .GroupBy(s => s.Sport)
                              .Select(scsc => new SportCategoriesSubCategoriesDTO.scsc_SportsDTO
                              {
                                  Sport = scsc.Key,
                                  Description = scsc.First().Description, // Assuming Description is the same for grouped Sports
                                  SubCategories = scsc.Where(sc => sc.SubCategory != null)
                                                     .Select(scsc => new SportCategoriesSubCategoriesDTO.scsc_SubCategoriesDTO
                                                     {
                                                         SubCategory = scsc.SubCategory
                                                     }).Distinct().ToList()
                              }).ToList()
                }).ToList();

            return Ok(groupedData);
        }


        // GET: api/Categories/5
        [HttpGet("{ID}")]
        public async Task<ActionResult<Sports>> GetSport(int ID)
        {
            var Sports = await _context.Sports.FindAsync(ID);

            if (Sports == null)
            {
                return NotFound();
            }

            return Sports;
        }

        // PUT: api/Categories/5
        [HttpPut("{ID}")]
        public async Task<IActionResult> UpdateSport(int ID, Sports Sport)
        {
            if (ID != Sport.ID)
            {
                return BadRequest();
            }

            _context.Entry(Sport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportExists(ID))
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

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Sports>> CreateSport(Sports Sport)
        {
            _context.Sports.Add(Sport);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSport), new { Sport.ID }, Sport);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{ID}")]
        public async Task<IActionResult> DeleteSport(int ID)
        {
            var Sport = await _context.Sports.FindAsync(ID);
            if (Sport == null)
            {
                return NotFound();
            }

            _context.Sports.Remove(Sport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SportExists(int ID)
        {
            return _context.Sports.Any(e => e.ID == ID);
        }
    }
}
