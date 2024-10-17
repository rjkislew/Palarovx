using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController : ControllerBase
    {
        private readonly palaro_2026Context _context;

        public SportsController(palaro_2026Context context)
        {
            _context = context;
        }

        // Get all categories
        [HttpGet("Categories")]
        public async Task<ActionResult<IEnumerable<Sports.s_CategoriesDTO>>> GetCategories()
        {
            var categories = await _context.sport_categories
                .Select(c => new Sports.s_CategoriesDTO { category = c.category })
                .ToListAsync();

            return Ok(categories);
        }

        // Get levels for a specific category using POST
        [HttpGet("Levels")]
        public async Task<ActionResult<IEnumerable<Sports.s_LevelsDTO>>> GetLevels([FromQuery] string category)
        {
            var levels = await (from sc in _context.sport_categories
                                join s in _context.sports on sc.id equals s.sports_category_id
                                join sub in _context.sport_sub_categories on s.id equals sub.sport_id
                                join l in _context.levels on sub.level_id equals l.id
                                where sc.category == category
                                select new Sports.s_LevelsDTO { level = l.level })
                                .Distinct().ToListAsync();

            return Ok(levels);
        }

        // Get sports for a specific category and level using POST
        [HttpGet("Sports")]
        public async Task<ActionResult<IEnumerable<Sports.s_SportsDTO>>> GetSports([FromQuery] string category, [FromQuery] string level)
        {
            var sports = await (from sc in _context.sport_categories
                                join s in _context.sports on sc.id equals s.sports_category_id
                                join sub in _context.sport_sub_categories on s.id equals sub.sport_id
                                join l in _context.levels on sub.level_id equals l.id
                                where sc.category == category && l.level == level
                                select new Sports.s_SportsDTO
                                {
                                    sport = s.sport,
                                    description = s.description
                                }).Distinct().ToListAsync();

            return Ok(sports);
        }

        // Get genders for a specific category, level, and sport using POST
        [HttpGet("Genders")]
        public async Task<ActionResult<IEnumerable<Sports.s_GendersDTO>>> GetGenders([FromQuery] string category, [FromQuery] string level, [FromQuery] string sport)
        {
            var genders = await (from sc in _context.sport_categories
                                 join s in _context.sports on sc.id equals s.sports_category_id
                                 join sub in _context.sport_sub_categories on s.id equals sub.sport_id
                                 join gc in _context.gender_categories on sub.gender_category_id equals gc.id
                                 join l in _context.levels on sub.level_id equals l.id
                                 where sc.category == category && l.level == level && s.sport == sport
                                 select new Sports.s_GendersDTO { gender_category = gc.gender_category })
                                 .Distinct().ToListAsync();

            return Ok(genders);
        }

        // Get sub-categories for a specific category, level, sport, and gender using POST
        [HttpGet("SubCategories")]
        public async Task<ActionResult<IEnumerable<Sports.s_SubCategoriesDTO>>> GetSubCategories([FromQuery] string category, [FromQuery] string level, [FromQuery] string sport, [FromQuery] string gender)
        {
            var subCategories = await (from sc in _context.sport_categories
                                       join s in _context.sports on sc.id equals s.sports_category_id
                                       join sub in _context.sport_sub_categories on s.id equals sub.sport_id
                                       join gc in _context.gender_categories on sub.gender_category_id equals gc.id
                                       join l in _context.levels on sub.level_id equals l.id
                                       where sc.category == category && l.level == level && s.sport == sport && gc.gender_category == gender
                                       select new Sports.s_SubCategoriesDTO { sub_category = sub.sub_category })
                                       .Distinct().ToListAsync();

            return Ok(subCategories);
        }

        [HttpGet("SportLevelsGendersSubCategories")]
        public async Task<ActionResult<IEnumerable<SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_CategoriesDTO>>> GetSportsDTO()
        {
            var sportsData = await (from sports_categories in _context.sport_categories
                                    join sports in _context.sports on sports_categories.id equals sports.sports_category_id into sportGroup
                                    from sport in sportGroup.DefaultIfEmpty()
                                    join sport_sub_categories in _context.sport_sub_categories on sport.id equals sport_sub_categories.sport_id into subGroup
                                    from sub in subGroup.DefaultIfEmpty()
                                    join levels in _context.levels on sub.level_id equals levels.id into levelGroup
                                    from level in levelGroup.DefaultIfEmpty()
                                    join gender_categories in _context.gender_categories on sub.gender_category_id equals gender_categories.id into genderGroup
                                    from gender in genderGroup.DefaultIfEmpty()
                                    select new
                                    {
                                        category = sports_categories != null ? sports_categories.category : null,
                                        level = level != null ? level.level : null,
                                        gender_category = gender != null ? gender.gender_category : null,
                                        sport = sport != null ? sport.sport : null,
                                        description = sport != null ? sport.description : null,
                                        sub_category = sub != null ? sub.sub_category : null
                                    }).ToListAsync();

            // Grouping logic to structure the data
            var groupedData = sportsData
                .GroupBy(x => x.category)
                .Select(g => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_CategoriesDTO
                {
                    category = g.Key,
                    levels = g.GroupBy(x => x.level)
                              .Select(l => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_LevelsDTO
                              {
                                  level = l.Key,
                                  sports = l.GroupBy(x => x.sport)
                                           .Select(sg => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_SportsDTO
                                           {
                                               sport = sg.Key,
                                               description = sg.First().description, // Use First() to get the description from grouped data
                                               gender_categories = sg.GroupBy(x => x.gender_category)
                                                                     .Select(gc => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_GendersDTO
                                                                     {
                                                                         gender_category = gc.Key,
                                                                         sub_categories = gc.Select(s => new SportCategoriesLevelsGendersSubCategoriesDTO.sclgsc_SubCategoriesDTO
                                                                         {
                                                                             sub_category = s.sub_category
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
            var categoriesData = await (from sports_categories in _context.sport_categories
                                        join sports in _context.sports on sports_categories.id equals sports.sports_category_id into sportGroup
                                        from sport in sportGroup.DefaultIfEmpty()
                                        select new
                                        {
                                            category = sports_categories.category,
                                            sport = sport != null ? sport.sport : null,
                                            description = sport != null ? sport.description : null
                                        }).ToListAsync();

            // Grouping logic to structure the data
            var groupedData = categoriesData
                .GroupBy(x => x.category)
                .Select(g => new SportCategoriesDTO.sc_CategoriesDTO
                {
                    category = g.Key,
                    sports = g.Where(x => x.sport != null)
                              .Select(s => new SportCategoriesDTO.sc_SportsDTO
                              {
                                  sport = s.sport,
                                  description = s.description
                              }).ToList()
                }).ToList();

            return Ok(groupedData);
        }

        [HttpGet("SportSubCategories")]
        public async Task<ActionResult<IEnumerable<SportCategoriesSubCategoriesDTO.scsc_CategoriesDTO>>> GetSportSubCategories()
        {
            var sportsData = await (from sports_categories in _context.sport_categories
                                    join sports in _context.sports on sports_categories.id equals sports.sports_category_id into sportGroup
                                    from sport in sportGroup.DefaultIfEmpty()
                                    join sport_sub_categories in _context.sport_sub_categories on sport.id equals sport_sub_categories.sport_id into subGroup
                                    from sub in subGroup.DefaultIfEmpty()
                                    select new
                                    {
                                        category = sports_categories.category,
                                        sport = sport != null ? sport.sport : null,
                                        description = sport != null ? sport.description : null,
                                        sub_category = sub != null ? sub.sub_category : null
                                    }).ToListAsync();

            // Grouping logic to structure the data
            var groupedData = sportsData
                .OrderByDescending(c => c.category)
                .GroupBy(c => c.category)
                .Select(s => new SportCategoriesSubCategoriesDTO.scsc_CategoriesDTO
                {
                    category = s.Key,
                    sports = s.Where(s => s.sport != null)
                              .GroupBy(s => s.sport)
                              .Select(scsc => new SportCategoriesSubCategoriesDTO.scsc_SportsDTO
                              {
                                  sport = scsc.Key,
                                  description = scsc.First().description, // Assuming description is the same for grouped sports
                                  sub_categories = scsc.Where(sc => sc.sub_category != null)
                                                     .Select(scsc => new SportCategoriesSubCategoriesDTO.scsc_SubCategoriesDTO
                                                     {
                                                         sub_category = scsc.sub_category
                                                     }).ToList()
                              }).ToList()
                }).ToList();

            return Ok(groupedData);
        }


        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<sports>> GetSport(int id)
        {
            var sport = await _context.sports.FindAsync(id);

            if (sport == null)
            {
                return NotFound();
            }

            return sport;
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSport(int id, sports sport)
        {
            if (id != sport.id)
            {
                return BadRequest();
            }

            _context.Entry(sport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportExists(id))
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
        public async Task<ActionResult<sports>> CreateSport(sports sport)
        {
            _context.sports.Add(sport);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSport), new { id = sport.id }, sport);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSport(int id)
        {
            var sport = await _context.sports.FindAsync(id);
            if (sport == null)
            {
                return NotFound();
            }

            _context.sports.Remove(sport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SportExists(int id)
        {
            return _context.sports.Any(e => e.id == id);
        }
    }
}
