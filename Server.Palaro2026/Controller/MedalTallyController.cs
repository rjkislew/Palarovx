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
        public async Task<ActionResult<List<MedalTallyDTO.RegionalMedalTally>>> GetMedalTally()
        {
            try
            {
                // Fetch all regions
                var allRegions = await _context.SchoolRegions
                    .Select(r => new { r.Region, r.Abbreviation })
                    .AsNoTracking()
                    .ToListAsync();

                // Fetch only Championship stage events
                var events = await _context.Events
                    .Include(e => e.EventVersusTeams)
                        .ThenInclude(ev => ev.SchoolRegion)
                    .Where(e => e.EventStage!.Stage == "Championship")
                    .AsNoTracking()
                    .ToListAsync();

                var medalTally = new Dictionary<string, MedalTallyDTO.RegionalMedalTally>();

                // Process medals for teams
                foreach (var eventEntity in events)
                {
                    var sortedTeams = eventEntity.EventVersusTeams?
                        .Where(ev => ev.SchoolRegion != null)
                        .OrderByDescending(ev => int.TryParse(ev.Score, out int score) ? score : 0)
                        .ToList();

                    if (sortedTeams == null || sortedTeams.Count == 0) continue;

                    // Assign medals
                    AssignMedal(sortedTeams.ElementAtOrDefault(0), medalTally, "Gold");
                    AssignMedal(sortedTeams.ElementAtOrDefault(1), medalTally, "Silver");
                    AssignMedal(sortedTeams.ElementAtOrDefault(2), medalTally, "Bronze");
                }

                // Ensure all regions are included, even if they have no medals
                foreach (var region in allRegions)
                {
                    if (!medalTally.ContainsKey(region.Region!))
                    {
                        medalTally[region.Region!] = new MedalTallyDTO.RegionalMedalTally
                        {
                            Region = region.Region!,
                            Abbreviation = region.Abbreviation!,
                            Gold = 0,
                            Silver = 0,
                            Bronze = 0,
                            Total = 0
                        };
                    }
                }

                // Convert dictionary to a sorted list
                var result = medalTally.Values
                    .OrderByDescending(mt => mt.Gold)
                    .ThenByDescending(mt => mt.Silver)
                    .ThenByDescending(mt => mt.Bronze)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper function to assign medals
        private void AssignMedal(EventVersusTeams? team, Dictionary<string, MedalTallyDTO.RegionalMedalTally> medalTally, string medalType)
        {
            if (team == null || team.SchoolRegion == null) return;

            string region = team.SchoolRegion.Region!;
            string abbreviation = team.SchoolRegion.Abbreviation!;

            if (!medalTally.ContainsKey(region))
            {
                medalTally[region] = new MedalTallyDTO.RegionalMedalTally
                {
                    Region = region,
                    Abbreviation = abbreviation,
                    Gold = 0,
                    Silver = 0,
                    Bronze = 0,
                    Total = 0
                };
            }

            switch (medalType)
            {
                case "Gold":
                    medalTally[region].Gold++;
                    break;
                case "Silver":
                    medalTally[region].Silver++;
                    break;
                case "Bronze":
                    medalTally[region].Bronze++;
                    break;
            }

            // Update total medal count
            medalTally[region].Total = (medalTally[region].Gold ?? 0) +
                                       (medalTally[region].Silver ?? 0) +
                                       (medalTally[region].Bronze ?? 0);
        }

    }
}
