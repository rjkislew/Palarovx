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

        [HttpGet] // /api/MedalTally
        public async Task<ActionResult<List<MedalTallyDTO.RegionalMedalTally>>> MedalTallyByRegion([FromQuery] string? region)
        {
            var regions = _context.SchoolRegions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(region))
            {
                regions = regions.Where(r => r.Region == region);
            }

            var medalTally = await regions
                .GroupJoin(
                    _context.EventVersusTeams
                        .Where(m => m.Rank == "Gold" || m.Rank == "Silver" || m.Rank == "Bronze"),
                    r => r.ID,
                    evt => evt.SchoolRegionID,
                    (r, medals) => new MedalTallyDTO.RegionalMedalTally
                    {
                        Region = r.Region!,
                        Abbreviation = r.Abbreviation!,
                        Gold = medals.Count(x => x.Rank == "Gold"),
                        Silver = medals.Count(x => x.Rank == "Silver"),
                        Bronze = medals.Count(x => x.Rank == "Bronze"),
                        Total = medals.Count()
                    }
                )
                .OrderByDescending(x => x.Gold)
                .ThenByDescending(x => x.Total)
                .ThenByDescending(x => x.Silver)
                .ThenByDescending(x => x.Bronze)
                .ThenBy(x => x.Region)
                .ToListAsync();

            return Ok(medalTally);
        }

        [HttpGet("BySchoolLevel")] // /api/MedalTally/BySchoolLevel
        public async Task<ActionResult<List<MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel>>> MedalTallyBySchoolLevel([FromQuery] string? region)
        {
            // Step 1: Get all school levels
            var allSchoolLevels = await _context.SchoolLevels
                .Select(sl => sl.Level)
                .Distinct()
                .ToListAsync();

            // Step 2: Get all regions (with optional filter)
            var allRegions = await _context.SchoolRegions
                .Where(r => string.IsNullOrWhiteSpace(region) || r.Region == region)
                .Select(r => new { r.Region, r.Abbreviation })
                .ToListAsync();

            // Step 3: Get medal tally data
            var medalTally = await _context.EventVersusTeams
                .Include(m => m.SchoolRegion)
                .Include(e => e.Event)
                    .ThenInclude(s => s!.SportSubcategory)
                        .ThenInclude(l => l!.SchoolLevel)
                .Where(m => m.Rank == "Gold" || m.Rank == "Silver" || m.Rank == "Bronze")
                .Where(m => string.IsNullOrWhiteSpace(region) || m.SchoolRegion!.Region == region)
                .GroupBy(m => new
                {
                    Level = m.Event!.SportSubcategory!.SchoolLevel!.Level,
                    Region = m.SchoolRegion!.Region,
                    Abbreviation = m.SchoolRegion.Abbreviation
                })
                .Select(g => new
                {
                    g.Key.Level,
                    g.Key.Region,
                    g.Key.Abbreviation,
                    Gold = g.Count(x => x.Rank == "Gold"),
                    Silver = g.Count(x => x.Rank == "Silver"),
                    Bronze = g.Count(x => x.Rank == "Bronze")
                })
                .ToListAsync();

            // Step 4: Compose final result: For each School Level, join all Regions (even with no medals)
            var result = allSchoolLevels
                .Select(level => new MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel
                {
                    Level = level,
                    RegionalMedalTallyList = allRegions
                        .GroupJoin(
                            medalTally.Where(m => m.Level == level),
                            region => region.Region,
                            medal => medal.Region,
                            (region, medals) => medals.Select(m => new MedalTallyDTO.SchoolLevelMedalTally.RegionalMedalTally
                            {
                                Region = m.Region,
                                Abbreviation = m.Abbreviation,
                                Gold = m.Gold,
                                Silver = m.Silver,
                                Bronze = m.Bronze,
                                Total = m.Gold + m.Silver + m.Bronze
                            }).DefaultIfEmpty(new MedalTallyDTO.SchoolLevelMedalTally.RegionalMedalTally
                            {
                                Region = region.Region,
                                Abbreviation = region.Abbreviation,
                                Gold = 0,
                                Silver = 0,
                                Bronze = 0,
                                Total = 0
                            }))
                        .SelectMany(x => x)
                        .OrderByDescending(x => x.Gold)
                        .ThenByDescending(x => x.Total)
                        .ThenByDescending(x => x.Silver)
                        .ThenByDescending(x => x.Bronze)
                        .ThenBy(x => x.Region)
                        .ToList()
                })
                .OrderBy(x => x.Level)
                .ToList();

            return result.Count == 0 ? NotFound("No medal tally found.") : Ok(result);
        }

    }
}
