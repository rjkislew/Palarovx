using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO.View;

namespace Server.Palaro2026.Controller.View
{
    [Route("api/[controller]")]
    [ApiController]
    public class vw_regional_teamsController(palaro_2026Context _context) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<vw_regional_teamsDTO>>> GetRegionalTeams()
        {
            var regions = await _context.vw_regional_teams.ToListAsync();

            var groupedRegions = regions
                .GroupBy(r => new { r.regional_team_name, r.regional_team_name_abbreviation })
                .Select(region => new vw_regional_teamsDTO
                {
                    regional_team_name = region.Key.regional_team_name,
                    regional_team_name_abbreviation = region.Key.regional_team_name_abbreviation,
                    division_name = region
                    .GroupBy(d => new { d.division_name })
                        .Select(division => new division_nameDTO
                        {
                            division_name = division.Key.division_name,
                            school_name = division
                            .Where(school => !string.IsNullOrEmpty(school.school_name)) // Ensure non-null and non-empty school_name
                            .Select(school => new school_nameDTO
                            {
                                school_name = school.school_name
                            }).ToList()
                        })
                        .Where(d => d.school_name?.Count > 0) // Filter out divisions with no schools
                        .ToList()
                })
                .Where(r => r.division_name?.Count > 0) // Filter out regions with no valid divisions
                .ToList();

            return groupedRegions;
        }

    }
}
