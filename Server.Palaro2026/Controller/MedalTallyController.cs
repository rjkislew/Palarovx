using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using Server.Palaro2026.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedalTallyController : ControllerBase
    {
        //private readonly Palaro2026Context _context;

        private readonly ISqlDataAccess _db;
        private readonly ILogger<MedalTallyController> _logger;

        //public MedalTallyController(Palaro2026Context context)
        //{
        //    _context = context;
        //}

        public MedalTallyController(ISqlDataAccess db, ILogger<MedalTallyController> logger)
        {
            _db = db;
            _logger = logger;
        }

        //[HttpGet] // /api/MedalTally
        //public async Task<ActionResult<List<MedalTallyDTO.RegionalMedalTally>>> MedalTallyByRegion([FromQuery] string? region)
        //{
        //    var regions = _context.SchoolRegions.AsQueryable();

        //    if (!string.IsNullOrWhiteSpace(region))
        //    {
        //        regions = regions.Where(r => r.Region == region);
        //    }

        //    var medalTally = await regions
        //        .GroupJoin(
        //            _context.EventVersusTeams
        //                .Where(m => m.Rank == "Gold" || m.Rank == "Silver" || m.Rank == "Bronze"),
        //            r => r.ID,
        //            evt => evt.SchoolRegionID,
        //            (r, medals) => new MedalTallyDTO.RegionalMedalTally
        //            {
        //                Region = r.Region!,
        //                Abbreviation = r.Abbreviation!,
        //                Gold = medals.Count(x => x.Rank == "Gold"),
        //                Silver = medals.Count(x => x.Rank == "Silver"),
        //                Bronze = medals.Count(x => x.Rank == "Bronze"),
        //                Total = medals.Count()
        //            }
        //        )
        //        .OrderByDescending(x => x.Gold)
        //        .ThenByDescending(x => x.Total)
        //        .ThenByDescending(x => x.Silver)
        //        .ThenByDescending(x => x.Bronze)
        //        .ThenBy(x => x.Region)
        //        .ToListAsync();

        //    return Ok(medalTally);
        //}

        [HttpGet]
        public async Task<IActionResult> MedalTallyByRegion([FromQuery] string? region)
        {
            try
            {
                string sql = @"
SELECT
    sr.Region,
    sr.Abbreviation,

    /* GOLD */
    SUM(CASE WHEN evt.Rank = 'Gold' AND evt.TeamID IS NULL THEN 1 ELSE 0 END)
    + COUNT(DISTINCT CASE
          WHEN evt.Rank = 'Gold' AND evt.TeamID IS NOT NULL
          THEN CONCAT(
                ISNULL(CAST(sc.SchoolLevelID AS varchar(20)), '0'), '|',
                ISNULL(CAST(e.SportSubcategoryID AS varchar(20)), '0'), '|',
                CAST(evt.TeamID AS varchar(50))
          )
      END) AS Gold,

    /* SILVER */
    SUM(CASE WHEN evt.Rank = 'Silver' AND evt.TeamID IS NULL THEN 1 ELSE 0 END)
    + COUNT(DISTINCT CASE
          WHEN evt.Rank = 'Silver' AND evt.TeamID IS NOT NULL
          THEN CONCAT(
                ISNULL(CAST(sc.SchoolLevelID AS varchar(20)), '0'), '|',
                ISNULL(CAST(e.SportSubcategoryID AS varchar(20)), '0'), '|',
                CAST(evt.TeamID AS varchar(50))
          )
      END) AS Silver,

    /* BRONZE */
    SUM(CASE WHEN evt.Rank = 'Bronze' AND evt.TeamID IS NULL THEN 1 ELSE 0 END)
    + COUNT(DISTINCT CASE
          WHEN evt.Rank = 'Bronze' AND evt.TeamID IS NOT NULL
          THEN CONCAT(
                ISNULL(CAST(sc.SchoolLevelID AS varchar(20)), '0'), '|',
                ISNULL(CAST(e.SportSubcategoryID AS varchar(20)), '0'), '|',
                CAST(evt.TeamID AS varchar(50))
          )
      END) AS Bronze,

    /* TOTAL */
    SUM(CASE WHEN evt.Rank IN ('Gold','Silver','Bronze') AND evt.TeamID IS NULL THEN 1 ELSE 0 END)
    + COUNT(DISTINCT CASE
          WHEN evt.Rank IN ('Gold','Silver','Bronze') AND evt.TeamID IS NOT NULL
          THEN CONCAT(
                ISNULL(CAST(sc.SchoolLevelID AS varchar(20)), '0'), '|',
                ISNULL(CAST(e.SportSubcategoryID AS varchar(20)), '0'), '|',
                CAST(evt.TeamID AS varchar(50))
          )
      END) AS Total

FROM SchoolRegions sr
LEFT JOIN EventVersusTeams evt
    ON sr.ID = evt.SchoolRegionID
   AND evt.Rank IN ('Gold', 'Silver', 'Bronze')
LEFT JOIN Events e
    ON e.ID = evt.EventID
   AND e.IsFinished = 1
LEFT JOIN SportSubcategories sc
    ON sc.ID = e.SportSubcategoryID

WHERE
    (@region IS NULL OR @region = ''
     OR sr.Region = @region
     OR sr.Abbreviation = @region)

GROUP BY sr.Region, sr.Abbreviation
ORDER BY Gold DESC, Total DESC, Silver DESC, Bronze DESC, sr.Region ASC;
";

                var result = await _db.QueryAsync<MedalTallyDTO.RegionalMedalTally, dynamic>(
                    sql,
                    new { region }
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medal tally by region");
                return StatusCode(500, "Error retrieving medal tally");
            }
        }

        [HttpGet("BySchoolLevel")] // /api/MedalTally/BySchoolLevel
        public async Task<ActionResult<List<MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel>>> MedalTallyBySchoolLevel([FromQuery] string? region)
        {
            try
            {
                // 1) All school levels
                string sqlLevels = @"
            SELECT DISTINCT [Level]
            FROM SchoolLevels
            ORDER BY [Level];";

                var allSchoolLevels = (await _db.QueryAsync<string, dynamic>(sqlLevels, new { }))
                    .ToList();

                // 2) All regions (optional filter)
                string sqlRegions = @"
            SELECT Region, Abbreviation
            FROM SchoolRegions
            WHERE (@region IS NULL OR @region = '' OR Region = @region)
            ORDER BY Region;";

                var allRegions = (await _db.QueryAsync<RegionRow, dynamic>(sqlRegions, new { region }))
                    .ToList();

                // 3) Medal tally grouped by Level + Region (✅ TeamID dedupe only when TeamID exists)
                string sqlMedals = @"
            SELECT
                sl.[Level] AS [Level],
                sr.Region AS Region,
                sr.Abbreviation AS Abbreviation,

                /* Gold */
                SUM(CASE WHEN evt.Rank = 'Gold' AND evt.TeamID IS NULL THEN 1 ELSE 0 END)
                + COUNT(DISTINCT CASE WHEN evt.Rank = 'Gold' AND evt.TeamID IS NOT NULL THEN evt.TeamID END) AS Gold,

                /* Silver */
                SUM(CASE WHEN evt.Rank = 'Silver' AND evt.TeamID IS NULL THEN 1 ELSE 0 END)
                + COUNT(DISTINCT CASE WHEN evt.Rank = 'Silver' AND evt.TeamID IS NOT NULL THEN evt.TeamID END) AS Silver,

                /* Bronze */
                SUM(CASE WHEN evt.Rank = 'Bronze' AND evt.TeamID IS NULL THEN 1 ELSE 0 END)
                + COUNT(DISTINCT CASE WHEN evt.Rank = 'Bronze' AND evt.TeamID IS NOT NULL THEN evt.TeamID END) AS Bronze

            FROM EventVersusTeams evt
            INNER JOIN SchoolRegions sr ON sr.ID = evt.SchoolRegionID
            INNER JOIN Events e ON e.ID = evt.EventID AND e.IsFinished = 1
            LEFT JOIN SportSubcategories sc ON sc.ID = e.SportSubcategoryID
            LEFT JOIN SchoolLevels sl ON sl.ID = sc.SchoolLevelID

            WHERE evt.Rank IN ('Gold','Silver','Bronze')
              AND (@region IS NULL OR @region = '' OR sr.Region = @region)

            GROUP BY
                sl.[Level],
                sr.Region,
                sr.Abbreviation;";

                var medalTally = (await _db.QueryAsync<MedalLevelRegionRow, dynamic>(sqlMedals, new { region }))
                    .ToList();

                // 4) Compose final nested result (same shape as your EF output)
                var result = allSchoolLevels
                    .Select(level => new MedalTallyDTO.SchoolLevelMedalTally.SchoolLevel
                    {
                        Level = level,
                        RegionalMedalTallyList = allRegions
                            .Select(r =>
                            {
                                var m = medalTally.FirstOrDefault(x => x.Level == level && x.Region == r.Region);

                                var gold = m?.Gold ?? 0;
                                var silver = m?.Silver ?? 0;
                                var bronze = m?.Bronze ?? 0;

                                return new MedalTallyDTO.SchoolLevelMedalTally.RegionalMedalTally
                                {
                                    Region = r.Region,
                                    Abbreviation = r.Abbreviation,
                                    Gold = gold,
                                    Silver = silver,
                                    Bronze = bronze,
                                    Total = gold + silver + bronze
                                };
                            })
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medal tally by school level");
                return StatusCode(500, "Error retrieving medal tally");
            }
        }

        // helper rows for Dapper mapping
        private class RegionRow
        {
            public string Region { get; set; } = "";
            public string Abbreviation { get; set; } = "";
        }

        private class MedalLevelRegionRow
        {
            public string? Level { get; set; }      // can be null if sc/school level missing
            public string Region { get; set; } = "";
            public string Abbreviation { get; set; } = "";
            public int Gold { get; set; }
            public int Silver { get; set; }
            public int Bronze { get; set; }
        }
    }
}

