using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;
using System.Data;

namespace Server.Palaro2026.Controller.Score
{
    [Route("api/score/[controller]")]
    [ApiController]
    public class TeamMatchController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<TeamMatchController> _logger;

        public TeamMatchController(ISqlDataAccess db, ILogger<TeamMatchController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET all team matches
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamMatchDTO>>> GetAll([FromQuery] string? eventId)
        {
            try
            {
                _logger.LogInformation("Fetching TeamMatch data with EventID: {EventID}", eventId ?? "null");

                string sql = @"
                    SELECT 
                        tm.ID,
                        tm.EventID,
                        tm.EventVersusID,
                        tm.RegionID,
                        tm.Phase,
                        tm.Score,
                        tm.IsWinner,
                        tm.IsSetWinner,
                        tm.CreatedAt,
                        e.SportMainCat AS EventName,
                        sr.Region AS Region,
                        sr.Abbreviation AS RegionAbbreviation,
                        s.Sport AS Sport,
                        es.Stage AS EventStage
                    FROM TeamMatch tm
                    LEFT JOIN Events e ON tm.EventID = e.ID
                    LEFT JOIN EventVersusTeams evt ON tm.EventVersusID = evt.ID
                    LEFT JOIN SchoolRegions sr ON evt.SchoolRegionID = sr.ID
                    LEFT JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
                    LEFT JOIN Sports s ON ssc.SportID = s.ID
                    LEFT JOIN EventStages es ON e.EventStageID = es.ID
                    WHERE (@EventID IS NULL OR tm.EventID = @EventID)
                    ORDER BY tm.EventID, tm.Phase;
                ";

                var data = await _db.QueryAsync<TeamMatchDTO, dynamic>(sql, new { EventID = eventId });

                _logger.LogInformation("Successfully fetched {Count} TeamMatch records", data?.Count() ?? 0);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching TeamMatch list");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET by EventVersusID (per team)
        [HttpGet("Team/{eventVersusId}")]
        public async Task<ActionResult<IEnumerable<TeamMatchDTO>>> GetByTeam(int eventVersusId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        tm.ID,
                        tm.EventID,
                        tm.EventVersusID,
                        tm.RegionID,
                        tm.Phase,
                        tm.Score,
                        tm.IsWinner,
                        tm.IsSetWinner,
                        tm.CreatedAt
                    FROM TeamMatch tm
                    WHERE tm.EventVersusID = @EventVersusID
                    ORDER BY tm.Phase;
                ";

                var matches = await _db.QueryAsync<TeamMatchDTO, dynamic>(sql, new { EventVersusID = eventVersusId });
                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching TeamMatch records for EventVersusID {EventVersusID}", eventVersusId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET by RegionID
        [HttpGet("Region/{regionId}")]
        public async Task<ActionResult<IEnumerable<TeamMatchDTO>>> GetByRegion(int regionId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        tm.ID,
                        tm.EventID,
                        tm.EventVersusID,
                        tm.RegionID,
                        tm.Phase,
                        tm.Score,
                        tm.IsWinner,
                        tm.IsSetWinner,
                        tm.CreatedAt,
                        e.SportMainCat AS EventName,
                        sr.Region AS Region,
                        sr.Abbreviation AS RegionAbbreviation,
                        s.Sport AS Sport
                    FROM TeamMatch tm
                    LEFT JOIN Events e ON tm.EventID = e.ID
                    LEFT JOIN EventVersusTeams evt ON tm.EventVersusID = evt.ID
                    LEFT JOIN SchoolRegions sr ON evt.SchoolRegionID = sr.ID
                    LEFT JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
                    LEFT JOIN Sports s ON ssc.SportID = s.ID
                    WHERE tm.RegionID = @RegionID
                    ORDER BY tm.EventID, tm.Phase;
                ";

                var matches = await _db.QueryAsync<TeamMatchDTO, dynamic>(sql, new { RegionID = regionId });
                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching TeamMatch records for RegionID {RegionID}", regionId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get team match per eventid
        [HttpGet("GetPhaseMatches")]
        public async Task<IActionResult> GetGeneratedPhaseMatches([FromQuery] string eventId, [FromQuery] int? phase = null)
        {
            if (string.IsNullOrEmpty(eventId))
                return BadRequest("EventID is required.");

            try
            {
                string sql = @"
                    SELECT 
                        tm.ID,
                        tm.EventID,
                        tm.EventVersusID,
                        tm.RegionID,
                        tm.Phase,
                        tm.Score,
                        tm.IsWinner,
                        tm.IsSetWinner,
                        tm.CreatedAt,
                        e.SportMainCat AS EventName,
                        sr.Region AS Region,
                        sr.Abbreviation AS RegionAbbreviation,
                        s.Sport AS Sport
                    FROM TeamMatch tm
                    LEFT JOIN EventVersusTeams evt ON tm.EventVersusID = evt.ID
                    LEFT JOIN Events e ON tm.EventID = e.ID
                    LEFT JOIN SchoolRegions sr ON evt.SchoolRegionID = sr.ID
                    LEFT JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
                    LEFT JOIN Sports s ON ssc.SportID = s.ID
                    WHERE tm.EventID = @EventID
                    AND (@Phase IS NULL OR tm.Phase = @Phase)
                    ORDER BY tm.Phase, tm.EventVersusID;
                ";

                var matches = await _db.QueryAsync<TeamMatchDTO, dynamic>(sql, new { EventID = eventId, Phase = phase });

                if (matches == null || !matches.Any())
                    return NotFound($"No TeamMatch records found for EventID {eventId}.");

                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching generated TeamMatch records for EventID {EventID}", eventId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get set scores for an event
        [HttpGet("SetScores/{eventId}")]
        public async Task<ActionResult<SetScoresDTO>> GetSetScores(string eventId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        Phase AS SetNumber,
                        SUM(CASE WHEN IsSetWinner = 1 THEN 1 ELSE 0 END) AS SetsWonByRegionA,
                        (SELECT COUNT(DISTINCT Phase) FROM TeamMatch WHERE EventID = @EventID AND IsSetWinner = 1) AS TotalSetsCompleted
                    FROM TeamMatch 
                    WHERE EventID = @EventID AND RegionID = (SELECT MIN(RegionID) FROM TeamMatch WHERE EventID = @EventID)
                    GROUP BY Phase
                    ORDER BY Phase;
                ";

                var setScores = await _db.QueryAsync<SetScoreInfo, dynamic>(sql, new { EventID = eventId });

                var result = new SetScoresDTO
                {
                    EventID = eventId,
                    SetScores = setScores.ToList(),
                    TotalSets = setScores.Count(),
                    CompletedSets = setScores.Count(s => s.SetsWonByRegionA > 0)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching set scores for EventID {EventID}", eventId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get team events
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<TeamEventDTO>>> GetTeamEvents(
            [FromQuery] string? sport = null,
            [FromQuery] string? subcategory = null,
            [FromQuery] string? gender = null,
            [FromQuery] string? level = null,
            [FromQuery] string? eventStage = null)
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT
                        e.ID,
                        s.Sport,
                        ssc.Subcategory,
                        sgc.Gender,
                        sl.Level,
                        es.Stage as EventStage
                    FROM Events e
                    INNER JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
                    INNER JOIN Sports s ON ssc.SportID = s.ID
                    INNER JOIN SportGenderCategories sgc ON ssc.SportGenderCategoryID = sgc.ID
                    INNER JOIN SchoolLevels sl ON ssc.SchoolLevelID = sl.ID
                    LEFT JOIN EventStages es ON e.EventStageID = es.ID
                    WHERE s.Sport IN ('baseball', 'basketball', 'badminton', 'football', 'futsal', 
                                    'softball', 'sepak takraw', 'table tennis', 'tennis', 'volleyball',
                                    'bocce', 'goal ball')";



                var conditions = new List<string>();
                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(sport))
                {
                    conditions.Add("s.Sport = @Sport");
                    parameters.Add("Sport", sport);
                }

                if (!string.IsNullOrEmpty(subcategory))
                {
                    conditions.Add("ssc.Subcategory = @Subcategory");
                    parameters.Add("Subcategory", subcategory);
                }

                if (!string.IsNullOrEmpty(gender))
                {
                    conditions.Add("sgc.Gender = @Gender");
                    parameters.Add("Gender", gender);
                }

                if (!string.IsNullOrEmpty(level))
                {
                    conditions.Add("sl.Level = @Level");
                    parameters.Add("Level", level);
                }

                if (!string.IsNullOrEmpty(eventStage))
                {
                    conditions.Add("es.Stage = @EventStage");
                    parameters.Add("EventStage", eventStage);
                }

                if (conditions.Any())
                {
                    sql += " AND " + string.Join(" AND ", conditions);
                }

                sql += " ORDER BY s.Sport, ssc.Subcategory, sgc.Gender, sl.Level";

                var events = await _db.QueryAsync<TeamEventDTO, dynamic>(sql, parameters);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching team events");
                return StatusCode(500, $"Error retrieving team events: {ex.Message}");
            }
        }

        // Generate Phase Matches
        [HttpPost("GeneratePhaseMatches")]
        public async Task<IActionResult> GeneratePhaseMatches([FromBody] CreateTeamMatchDTO data)
        {
            if (string.IsNullOrEmpty(data.EventID))
                return BadRequest("EventID is required.");

            try
            {
                string getVersusSql = @"
                    SELECT ID, SchoolRegionID 
                    FROM EventVersusTeams
                    WHERE EventID = @EventID;
                ";

                var versusList = await _db.QueryAsync<(int Id, int SchoolRegionID), dynamic>(getVersusSql, new { data.EventID });
                var versusData = versusList.ToList();

                if (!versusData.Any())
                    return NotFound($"No EventVersusTeams found for EventID {data.EventID}.");

                string insertSql = @"
                    INSERT INTO TeamMatch (EventID, EventVersusID, RegionID, Phase, Score, IsWinner, IsSetWinner, CreatedAt)
                    VALUES (@EventID, @EventVersusID, @RegionID, @Phase, @Score, @IsWinner, @IsSetWinner, GETDATE());
                ";

                foreach (var (eventVersusId, schoolRegionId) in versusData)
                {
                    await _db.ExecuteAsync<dynamic>(insertSql, new
                    {
                        EventID = data.EventID,
                        EventVersusID = eventVersusId,
                        RegionID = schoolRegionId,
                        Phase = data.Phase,
                        Score = data.Score,
                        IsWinner = data.IsWinner,
                        IsSetWinner = data.IsSetWinner
                    });
                }

                return Ok(new
                {
                    message = $"{versusData.Count} TeamMatch records created for EventID {data.EventID}.",
                    phase = data.Phase,
                    eventID = data.EventID
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating TeamMatch records for EventID {EventID}", data.EventID);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST new match score (per phase)
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateTeamMatchDTO data)
        {
            try
            {
                if (data == null)
                    return BadRequest("Invalid match data.");

                string sql = @"
                    INSERT INTO TeamMatch (EventID, EventVersusID, RegionID, Phase, Score, IsWinner, IsSetWinner, CreatedAt)
                    VALUES (@EventID, @EventVersusID, @RegionID, @Phase, @Score, @IsWinner, @IsSetWinner, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() as int);
                ";

                int newId = await _db.ExecuteScalarAsync<int, dynamic>(sql, new
                {
                    data.EventID,
                    data.EventVersusID,
                    data.RegionID,
                    data.Phase,
                    data.Score,
                    data.IsWinner,
                    data.IsSetWinner
                });

                if (newId > 0)
                    return Ok(newId);

                return BadRequest("Failed to insert TeamMatch.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating TeamMatch record.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Mark set winner for a specific phase
        [HttpPost("MarkSetWinner")]
        public async Task<IActionResult> MarkSetWinner([FromBody] MarkSetWinnerDTO data)
        {
            try
            {
                if (data == null || string.IsNullOrEmpty(data.EventID))
                    return BadRequest("Invalid data.");

                string resetSql = @"
            UPDATE TeamMatch 
            SET IsSetWinner = 0 
            WHERE EventID = @EventID AND Phase = @Phase;
        ";

                await _db.ExecuteAsync<dynamic>(resetSql, new
                {
                    data.EventID,
                    data.Phase
                });

                if (data.WinningRegionID > 0)
                {
                    string setWinnerSql = @"
                UPDATE TeamMatch 
                SET IsSetWinner = 1 
                WHERE EventID = @EventID AND Phase = @Phase AND RegionID = @WinningRegionID;
            ";

                    await _db.ExecuteAsync<dynamic>(setWinnerSql, new
                    {
                        data.EventID,
                        data.Phase,
                        data.WinningRegionID
                    });

                    return Ok($"Set {data.Phase} marked as won by region {data.WinningRegionID} for event {data.EventID}.");
                }
                else
                {
                    return Ok($"Set {data.Phase} winner status reset for event {data.EventID}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking set winner for EventID {EventID}, Phase {Phase}", data.EventID, data.Phase);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Update score/winner per phase
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeamMatchDTO data)
        {
            try
            {
                if (data == null)
                    return BadRequest("Invalid update data.");

                string sql = @"
                    UPDATE TeamMatch
                    SET 
                        Score = ISNULL(@Score, Score),
                        IsWinner = ISNULL(@IsWinner, IsWinner),
                        IsSetWinner = ISNULL(@IsSetWinner, IsSetWinner),
                        RegionID = ISNULL(@RegionID, RegionID)
                    WHERE ID = @ID;
                ";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    ID = id,
                    data.Score,
                    data.IsWinner,
                    data.IsSetWinner,
                    data.RegionID
                });

                return Ok("TeamMatch updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating TeamMatch with ID {ID}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Bulk update scores for multiple matches
        [HttpPut("BulkUpdate")]
        public async Task<IActionResult> BulkUpdate([FromBody] List<UpdateTeamMatchDTO> updates)
        {
            try
            {
                if (updates == null || !updates.Any())
                    return BadRequest("No update data provided.");

                string sql = @"
                    UPDATE TeamMatch
                    SET 
                        Score = ISNULL(@Score, Score),
                        IsWinner = ISNULL(@IsWinner, IsWinner),
                        IsSetWinner = ISNULL(@IsSetWinner, IsSetWinner)
                    WHERE ID = @ID;
                ";

                foreach (var update in updates)
                {
                    await _db.ExecuteAsync<dynamic>(sql, new
                    {
                        update.ID,
                        update.Score,
                        update.IsWinner,
                        update.IsSetWinner
                    });
                }

                return Ok($"{updates.Count} TeamMatch records updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating TeamMatch records");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE a phase record
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string sql = "DELETE FROM TeamMatch WHERE ID = @ID;";
                await _db.ExecuteAsync<dynamic>(sql, new { ID = id });

                return Ok("TeamMatch deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting TeamMatch with ID {ID}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}