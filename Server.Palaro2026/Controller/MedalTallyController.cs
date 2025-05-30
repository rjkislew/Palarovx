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

        [HttpGet]
        public async Task<ActionResult<List<MedalTallyDTO.RegionalMedalTally>>> MedalTallyByRegion([FromQuery] string? region)
        {
            var query = _context.EventVersusTeams
                .Include(m => m.SchoolRegion)
                .Where(m => m.Rank == "Champion" || m.Rank == "First Runner-up" || m.Rank == "Second Runner-up");

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
                .OrderByDescending(x => x.Gold)
                .ThenBy(x => x.Region)
                .ThenByDescending(x => x.Silver)
                .ThenBy(x => x.Region)
                .ThenByDescending(x => x.Bronze)
                .ThenBy(x => x.Region)
                .ToListAsync();

            return Ok(medalTally);
        }

        [HttpGet("BySchoolLevel")]
        public async Task<ActionResult<List<MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel>>> MedalTallyBySchoolLevel([FromQuery] string? region)
        {
            var medalTally = await _context.EventVersusTeams
                .Include(m => m.SchoolRegion)
                .Include(e => e.Event)
                    .ThenInclude(s => s!.SportSubcategory)
                        .ThenInclude(l => l!.SchoolLevel)
                .Where(m => m.Rank == "Champion" || m.Rank == "First Runner-up" || m.Rank == "Second Runner-up")
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
                    Gold = g.Count(x => x.Rank == "Champion"),
                    Silver = g.Count(x => x.Rank == "First Runner-up"),
                    Bronze = g.Count(x => x.Rank == "Second Runner-up")
                })
                .ToListAsync();

            var result = medalTally
                .GroupBy(x => x.Level)
                .Select(g => new MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel
                {
                    Level = g.Key,
                    RegionalMedalTallyList = g
                        .Select(x => new MedalTallyDTO.SchoolLevelMedalTally.RegionalMedalTally
                        {
                            Region = x.Region,
                            Abbreviation = x.Abbreviation,
                            Gold = x.Gold,
                            Silver = x.Silver,
                            Bronze = x.Bronze,
                            Total = x.Gold + x.Silver + x.Bronze
                        })
                        .OrderByDescending(x => x.Gold)
                        .ThenBy(x => x.Region)
                        .ThenByDescending(x => x.Silver)
                        .ThenBy(x => x.Region)
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
