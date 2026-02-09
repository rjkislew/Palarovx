using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;

namespace Server.Palaro2026.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class TabulationController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<TabulationController> _logger;

        public TabulationController(ISqlDataAccess db, ILogger<TabulationController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/Tabulation/result
        [HttpGet("result")]
        public async Task<IActionResult> GetUnifiedVersusRanking()
        {
            try
            {
                string sql = @"
;WITH EventSource AS
(
    SELECT
        'EVENT' AS SourceType,
        e.ID AS EventID,
        e.EventStageID,
        e.SportSubcategoryID,
        e.SportMainCat,
        e.IsFinished,
        NULL AS PerformanceID,
        evt.ID AS EventVersusTeamID,
        evt.SchoolRegionID,
        NULL AS TeamID, -- EVENT: teamid is null
        CAST(evt.Rank AS VARCHAR(20)) AS TeamRank,
        evt.PerformanceScoreID,

        -- ✅ LEVEL (from SportSubcategories -> SchoolLevels)
        sc.SchoolLevelID,
        sl.Level,

        pp.ID AS PlayerID,
        pp.FirstName,
        pp.LastName,
        pp.MiddleInitial,
        pp.Sex,
        pp.BirthDate,
        pp.SchoolID,
        pp.SportID,
        pp.ImagePath
    FROM Events e
    INNER JOIN EventVersusTeams evt ON evt.EventID = e.ID
    INNER JOIN EventVersusTeamPlayers evp ON evp.EventVersusID = evt.ID
    INNER JOIN ProfilePlayers pp ON pp.ID = evp.ProfilePlayerID
    LEFT JOIN SportSubcategories sc ON sc.ID = e.SportSubcategoryID
    LEFT JOIN SchoolLevels sl ON sl.ID = sc.SchoolLevelID
    WHERE e.IsFinished = 1
      AND evt.PerformanceScoreID IS NULL
),
PerformanceSource_WithVersus AS
(
    SELECT
        'PERFORMANCE' AS SourceType,
        NULL AS EventID,
        pe.StageID AS EventStageID,
        ps.SportSubcategoryID,
        pe.MainCategory AS SportMainCat,
        pe.IsFinished,
        pe.ID AS PerformanceID,
        evt.ID AS EventVersusTeamID,
        evt.SchoolRegionID,
        pt.TeamID AS TeamID, -- PERFORMANCE: store teamid from PerformanceTeam
        CAST(evt.Rank AS VARCHAR(20)) AS TeamRank,
        evt.PerformanceScoreID,

        -- ✅ LEVEL (from SportSubcategories -> SchoolLevels)
        sc.SchoolLevelID,
        sl.Level,

        pp.ID AS PlayerID,
        pp.FirstName,
        pp.LastName,
        pp.MiddleInitial,
        pp.Sex,
        pp.BirthDate,
        pp.SchoolID,
        pp.SportID,
        pp.ImagePath
    FROM EventVersusTeams evt
    INNER JOIN PerformanceScore ps ON ps.ID = evt.PerformanceScoreID
    INNER JOIN PerformanceTeam pt ON pt.ID = ps.PerformanceTeamID
    INNER JOIN ProfilePlayers pp ON pp.ID = pt.PlayerID
    INNER JOIN PerformanceEvent pe ON pe.ID = ps.PerformanceID
    LEFT JOIN SportSubcategories sc ON sc.ID = ps.SportSubcategoryID
    LEFT JOIN SchoolLevels sl ON sl.ID = sc.SchoolLevelID
    WHERE pe.IsFinished = 1
      AND evt.EventID IS NULL
      AND evt.PerformanceScoreID IS NOT NULL
),
PerformanceSource_NoVersus AS
(
    SELECT
        'PERFORMANCE' AS SourceType,
        NULL AS EventID,
        pe.StageID AS EventStageID,
        ps.SportSubcategoryID,
        pe.MainCategory AS SportMainCat,
        pe.IsFinished,
        pe.ID AS PerformanceID,
        NULL AS EventVersusTeamID,
        pt.RegionID AS SchoolRegionID,
        pt.TeamID AS TeamID, -- PERFORMANCE (no versus): store teamid from PerformanceTeam
        CAST(ps.Rank AS VARCHAR(20)) AS TeamRank,
        NULL AS PerformanceScoreID,

        -- ✅ LEVEL (from SportSubcategories -> SchoolLevels)
        sc.SchoolLevelID,
        sl.Level,

        pp.ID AS PlayerID,
        pp.FirstName,
        pp.LastName,
        pp.MiddleInitial,
        pp.Sex,
        pp.BirthDate,
        pp.SchoolID,
        pp.SportID,
        pp.ImagePath
    FROM PerformanceScore ps
    INNER JOIN PerformanceTeam pt ON pt.ID = ps.PerformanceTeamID
    INNER JOIN ProfilePlayers pp ON pp.ID = pt.PlayerID
    INNER JOIN PerformanceEvent pe ON pe.ID = ps.PerformanceID
    LEFT JOIN EventVersusTeams evt ON evt.PerformanceScoreID = ps.ID
    LEFT JOIN SportSubcategories sc ON sc.ID = ps.SportSubcategoryID
    LEFT JOIN SchoolLevels sl ON sl.ID = sc.SchoolLevelID
    WHERE pe.IsFinished = 1
      AND evt.ID IS NULL
),
Combined AS
(
    SELECT * FROM EventSource
    UNION ALL
    SELECT * FROM PerformanceSource_WithVersus
    UNION ALL
    SELECT * FROM PerformanceSource_NoVersus
),
Ranked AS
(
    SELECT
        c.*,
        CASE
            WHEN c.TeamRank IN ('Gold','Silver','Bronze') THEN 0
            WHEN TRY_CAST(c.TeamRank AS INT) IS NOT NULL
                 AND TRY_CAST(c.TeamRank AS INT) > 0 THEN 1
            ELSE 2
        END AS RankGroup,
        CASE
            WHEN c.TeamRank = 'Gold' THEN 1
            WHEN c.TeamRank = 'Silver' THEN 2
            WHEN c.TeamRank = 'Bronze' THEN 3
            ELSE 999
        END AS MedalOrder,
        CASE
            WHEN TRY_CAST(c.TeamRank AS INT) IS NOT NULL
                 AND TRY_CAST(c.TeamRank AS INT) > 0
                THEN TRY_CAST(c.TeamRank AS INT)
            ELSE 999
        END AS NumericOrder,
        ROW_NUMBER() OVER
        (
            PARTITION BY
                c.PlayerID,
                c.SportSubcategoryID,
                COALESCE(CONVERT(VARCHAR(50), c.PerformanceID),
                         CONVERT(VARCHAR(50), c.EventID))
            ORDER BY
                ISNULL(c.EventStageID, 0) DESC,
                CASE
                    WHEN c.TeamRank IN ('Gold','Silver','Bronze') THEN 0
                    WHEN TRY_CAST(c.TeamRank AS INT) IS NOT NULL
                         AND TRY_CAST(c.TeamRank AS INT) > 0 THEN 1
                    ELSE 2
                END,
                CASE
                    WHEN c.TeamRank = 'Gold' THEN 1
                    WHEN c.TeamRank = 'Silver' THEN 2
                    WHEN c.TeamRank = 'Bronze' THEN 3
                    ELSE 999
                END,
                CASE
                    WHEN TRY_CAST(c.TeamRank AS INT) IS NOT NULL
                         AND TRY_CAST(c.TeamRank AS INT) > 0
                        THEN TRY_CAST(c.TeamRank AS INT)
                    ELSE 999
                END
        ) AS rn_global
    FROM Combined c
)
SELECT *
FROM Ranked
WHERE rn_global = 1
ORDER BY
    ISNULL(EventStageID, 0) DESC,
    RankGroup,
    MedalOrder,
    NumericOrder,
    LastName;
";

                var data = await _db.QueryAsync<TabulationResultDTO, dynamic>(sql, new { });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unified versus ranking");
                return StatusCode(500, "Error retrieving unified versus ranking");
            }
        }
    }
}
