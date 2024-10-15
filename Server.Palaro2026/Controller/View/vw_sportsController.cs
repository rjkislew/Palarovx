using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO.View;

namespace Server.Palaro2026.Controller.View
{
    [Route("api/[controller]")]
    [ApiController]
    public class vw_sportsController : ControllerBase
    {
        private readonly palaro_2026Context _context;

        public vw_sportsController(palaro_2026Context context)
        {
            _context = context;
        }

        [HttpGet("sportsSubCategoryTree")]
        public async Task<ActionResult<IEnumerable<vw_categoryDTO>>> GetSportsListTree()
        {
            var categories = await _context.vw_sports.AsNoTracking().ToListAsync();

            var groupedCategory = categories
                .OrderByDescending(c => c.category)
                .GroupBy(c => new { c.category })
                .Select(category => new vw_categoryDTO
                {
                    category = category.Key.category,
                    level = category
                    .GroupBy(l => new { l.level })
                    .Select(level => new levelDTO
                    {
                        level = level.Key.level,
                        gender_category = level
                        .GroupBy(g => new { g.gender_category })
                        .Select(gender => new gender_categoryDTO
                        {
                            gender_category = gender.Key.gender_category,
                            sport = gender
                            .GroupBy(s => new { s.sport, s.description })
                            .Select(sport => new sportDTO
                            {
                                sport = sport.Key.sport,
                                description = sport.Key.description,
                                sub_category = sport
                                .Select(sub_sport => new sub_categoryDTO
                                {
                                    sub_category = sub_sport.sub_category,
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

            return groupedCategory;
        }

        [HttpGet("sportsListwithCategory")]
        public async Task<ActionResult<IEnumerable<vw_sports_with_sub_categoriesDTO>>> GetSubSportsList()
        {
            var categories = await _context.vw_sports.AsNoTracking().ToListAsync();

            var groupedCategory = categories
                .OrderByDescending(c => c.category)
                .GroupBy(c => new { c.category })
                .Select(main => new vw_sports_with_sub_categoriesDTO
                {
                    category = main.Key.category,
                    sports = main
                    .GroupBy(s => new { s.sport, s.description })
                    .Select(sub => new sport_list_with_sub_categoriesDTO
                    {
                        sport = sub.Key.sport,
                        description = sub.Key.description,
                        sub_categories = sub
                        .GroupBy(ss => new { ss.sub_category })
                        .Select(sub_sport => new sub_category_with_sub_categoriesDTO
                        {
                            sub_category = sub_sport.Key.sub_category,
                        }).ToList()
                    }).ToList()
                }).ToList();

            return groupedCategory;
        }
    }
}
