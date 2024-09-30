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
            var categories = await _context.vw_sports.ToListAsync();

            var groupedCategory = categories
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
                            .GroupBy(s => new { s.sport })
                            .Select(sport => new sportDTO
                            {
                                sport = sport.Key.sport,
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

        [HttpGet("categoryAndSportsList")]
        public async Task<ActionResult<IEnumerable<vw_category_sub_categoryDTO>>> GetSubSportsList()
        {
            var categories = await _context.vw_sports.ToListAsync();

            var groupedCategory = categories
                .GroupBy(c => new { c.category })
                .Select(main => new vw_category_sub_categoryDTO
                {
                    category = main.Key.category,
                    sports = main
                    .GroupBy(s => new { s.sport })
                    .Select(sub => new sportDTO_listDTO
                    {
                        sport = sub.Key.sport
                    }).ToList()
                }).ToList();

            return groupedCategory;
        }
    }
}
