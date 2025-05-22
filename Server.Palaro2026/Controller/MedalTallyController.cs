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
    }
}
