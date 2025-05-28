using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;


namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedalTallyController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public MedalTallyController(Palaro2026Context context)
        {
            _context = context;
        }

        [HttpGet("ByRegion")]
        public async Task<ActionResult<List<MedalTallyDTO.RegionalMedalTally>>> MedalTallyByRegion([FromQuery] string? region)
        {
            var query = _context.EventVersusTeams
                .Include(m => m.SchoolRegion)
                .Where(m => m.Rank == "Champion" || m.Rank == "First Runner-up" || m.Rank == "Second Runner-up");

            // Apply region filter if specified
            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(m => m.SchoolRegion!.Region == region);
            }

            var medalTally = await query
                .GroupBy(m => new { m.SchoolRegion!.Region, m.SchoolRegion.Abbreviation })
                .Select(g => new MedalTallyDTO.RegionalMedalTally
                {
                    Region = g.Key.Region,
                    Abbreviation = g.Key.Abbreviation,
                    Gold = g.Count(x => x.Rank == "Champion"),
                    Silver = g.Count(x => x.Rank == "First Runner-up"),
                    Bronze = g.Count(x => x.Rank == "Second Runner-up"),
                    Total = g.Count()
                })
                .ToListAsync();

            if (medalTally == null || medalTally.Count == 0)
            {
                return NotFound("No medal tally found.");
            }

            return Ok(medalTally);
        }


        [HttpGet("BySchoolLevel")]
        public async Task<ActionResult<List<MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel>>> MedalTallyBySchoolLevel([FromQuery] string? region)
        {
            var query = _context.EventVersusTeams
                .Include(m => m.SchoolRegion)
                .Include(e => e.Event)
                    .ThenInclude(s => s!.SportSubcategory)
                        .ThenInclude(l => l!.SchoolLevel)
                .Where(m => m.Rank == "Champion" || m.Rank == "First Runner-up" || m.Rank == "Second Runner-up");

            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(m => m.SchoolRegion!.Region == region);
            }

            // Fetch the data into memory
            var data = await query.ToListAsync();

            // Group in memory
            var groupedByLevel = data
                .GroupBy(m => new
                {
                    Level = m.Event!.SportSubcategory!.SchoolLevel!.Level,
                    Region = m.SchoolRegion!.Region,
                    Abbreviation = m.SchoolRegion.Abbreviation
                })
                .GroupBy(g => g.Key.Level)
                .Select(g => new MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel
                {
                    Level = g.Key,
                    RegionalMedalTallyList = g.Select(regionGroup => new MedalTallyDTO.SchoolLevelMedalTally.RegionalMedalTally
                    {
                        Region = regionGroup.Key.Region,
                        Abbreviation = regionGroup.Key.Abbreviation,
                        Gold = regionGroup.Count(x => x.Rank == "Champion"),
                        Silver = regionGroup.Count(x => x.Rank == "First Runner-up"),
                        Bronze = regionGroup.Count(x => x.Rank == "Second Runner-up"),
                        Total = regionGroup.Count()
                    }).ToList()
                })
                .ToList();

            if (groupedByLevel == null || groupedByLevel.Count == 0)
            {
                return NotFound("No medal tally found.");
            }

            return Ok(groupedByLevel);
        }

    }
}
