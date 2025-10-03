using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class SportsController : ControllerBase
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
                                                Subcategory = sub.Subcategory,
                                                MainCategory = sub.MainCategory
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
    }
}
