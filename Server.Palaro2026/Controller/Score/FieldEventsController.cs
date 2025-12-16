using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;
using System.Data;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FieldEventsController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<FieldEventsController> _logger;

        public FieldEventsController(ISqlDataAccess db, ILogger<FieldEventsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET Athletics Field Events
        [HttpGet("Sports/AthleticsFieldEvents")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAthleticsFieldEvents(
            [FromQuery] string? subcategory = null,
            [FromQuery] string? gender = null,
            [FromQuery] string? level = null,
            [FromQuery] string? eventStage = null)
        {
            try
            {
                var sql = @"
                    SELECT 
                        e.ID,
                        e.Date,
                        e.Time,
                        s.Sport,
                        ssc.Subcategory,
                        sgc.Gender,
                        sl.Level,
                        es.Stage as EventStage,
                        e.SportMainCat,
                        sr.Region,
                        sr.Abbreviation,
                        sr.ID as RegionID,
                        evtp.ProfilePlayerID as PlayerID,
                        pp.FirstName + ' ' + pp.LastName as PlayerName
                    FROM Events e
                    INNER JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
                    INNER JOIN Sports s ON ssc.SportID = s.ID
                    INNER JOIN SportGenderCategories sgc ON ssc.SportGenderCategoryID = sgc.ID
                    INNER JOIN SchoolLevels sl ON ssc.SchoolLevelID = sl.ID
                    LEFT JOIN EventStages es ON e.EventStageID = es.ID
                    LEFT JOIN EventVersusTeams evt ON e.ID = evt.EventID
                    LEFT JOIN SchoolRegions sr ON evt.SchoolRegionID = sr.ID
                    LEFT JOIN EventVersusTeamPlayers evtp ON evt.ID = evtp.EventVersusID
                    LEFT JOIN ProfilePlayers pp ON evtp.ProfilePlayerID = pp.ID
                    WHERE s.Sport = 'Athletics' 
                    AND e.SportMainCat = 'Field Events'";

                var conditions = new List<string>();
                var parameters = new DynamicParameters();

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

                sql += " ORDER BY e.Date, e.Time, ssc.Subcategory";

                var events = await _db.QueryAsync<dynamic, dynamic>(sql, parameters);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching athletics field events");
                return StatusCode(500, $"Error retrieving events: {ex.Message}");
            }
        }

        // POST field event results
        [HttpPost("SaveResults")]
        public async Task<IActionResult> SaveFieldEventResults([FromBody] FieldEventsDTO.SaveFieldEventsRequest request)
        {
            try
            {
                string checkEventSql = "SELECT COUNT(1) FROM Events WHERE ID = @EventID";
                var eventCount = await _db.ExecuteScalarAsync<int, dynamic>(
                    checkEventSql,
                    new { EventID = request.EventID });

                if (eventCount == 0)
                    return NotFound($"Event with ID '{request.EventID}' not found.");

                await _db.ExecuteAsync<dynamic>(
                    "DELETE FROM FieldEvents WHERE EventID = @EventID",
                    new { EventID = request.EventID });

                foreach (var result in request.Results)
                {
                    string insertSql = @"
                        INSERT INTO FieldEvents (EventID, RegionID, PlayerID, AttemptNo, Result, Rank, CreatedAt, UpdatedAt)
                        VALUES (@EventID, @RegionID, @PlayerID, @AttemptNo, @Result, @Rank, GETDATE(), GETDATE())";

                    await _db.ExecuteAsync<dynamic>(
                        insertSql,
                        new
                        {
                            EventID = request.EventID,
                            result.RegionID,
                            result.PlayerID,
                            result.AttemptNo,
                            result.Result,
                            result.Rank
                        });
                }

                return Ok(new { message = "Field event results saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving field event results for EventID {EventID}", request.EventID);
                return StatusCode(500, $"Error saving results: {ex.Message}");
            }
        }

        // GET field event results for an event
        [HttpGet("GetResults/{eventId}")]
        public async Task<ActionResult<IEnumerable<FieldEventsDTO.FieldEventResult>>> GetFieldEventResults(string eventId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        EventID,
                        RegionID,
                        PlayerID,
                        AttemptNo,
                        Result,
                        Rank
                    FROM FieldEvents
                    WHERE EventID = @EventID
                    ORDER BY RegionID, AttemptNo";

                var results = await _db.QueryAsync<FieldEventsDTO.FieldEventResult, dynamic>(
                    sql,
                    new { EventID = eventId });

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching field event results for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving results: {ex.Message}");
            }
        }

        // GET rankings for an event
        [HttpGet("GetRankings/{eventId}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetFieldEventRankings(string eventId)
        {
            try
            {
                string sql = @"
                    WITH BestResults AS (
                        SELECT 
                            RegionID,
                            PlayerID,
                            MAX(Result) as BestResult
                        FROM FieldEvents
                        WHERE EventID = @EventID
                        GROUP BY RegionID, PlayerID
                    ),
                    Rankings AS (
                        SELECT 
                            br.RegionID,
                            br.PlayerID,
                            br.BestResult,
                            sr.Abbreviation as RegionAbbreviation,
                            pp.FirstName + ' ' + pp.LastName as PlayerName,
                            DENSE_RANK() OVER (ORDER BY br.BestResult DESC) as Rank
                        FROM BestResults br
                        LEFT JOIN SchoolRegions sr ON br.RegionID = sr.ID
                        LEFT JOIN ProfilePlayers pp ON br.PlayerID = pp.ID
                        WHERE br.BestResult IS NOT NULL
                    )
                    SELECT * FROM Rankings
                    ORDER BY Rank";

                var rankings = await _db.QueryAsync<dynamic, dynamic>(sql, new { EventID = eventId });
                return Ok(rankings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rankings for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving rankings: {ex.Message}");
            }
        }

        // Update rankings for an event
        [HttpPost("UpdateRankings/{eventId}")]
        public async Task<IActionResult> UpdateFieldEventRankings(string eventId)
        {
            try
            {
                string bestResultsSql = @"
                    SELECT 
                        RegionID,
                        PlayerID,
                        MAX(Result) as BestResult
                    FROM FieldEvents
                    WHERE EventID = @EventID
                    GROUP BY RegionID, PlayerID
                    ORDER BY MAX(Result) DESC";

                var bestResults = await _db.QueryAsync<dynamic, dynamic>(
                    bestResultsSql,
                    new { EventID = eventId });

                int rank = 1;
                foreach (var result in bestResults)
                {
                    string updateSql = @"
                        UPDATE FieldEvents 
                        SET [Rank] = @Rank, UpdatedAt = GETDATE()
                        WHERE EventID = @EventID 
                        AND RegionID = @RegionID 
                        AND (@PlayerID IS NULL OR PlayerID = @PlayerID)";

                    await _db.ExecuteAsync<dynamic>(
                        updateSql,
                        new
                        {
                            EventID = eventId,
                            result.RegionID,
                            result.PlayerID,
                            Rank = rank++
                        });
                }

                return Ok(new { message = "Rankings updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rankings for EventID {EventID}", eventId);
                return StatusCode(500, $"Error updating rankings: {ex.Message}");
            }
        }

        // GET specific attempts
        [HttpGet("GetParticipantAttempts/{eventId}/{regionId}")]
        public async Task<ActionResult<IEnumerable<FieldEventsDTO.FieldEventResult>>> GetParticipantAttempts(string eventId, int regionId, [FromQuery] string? playerId = null)
        {
            try
            {
                string sql = @"
                    SELECT 
                        EventID,
                        RegionID,
                        PlayerID,
                        AttemptNo,
                        Result,
                        Rank
                    FROM FieldEvents
                    WHERE EventID = @EventID 
                    AND RegionID = @RegionID
                    AND (@PlayerID IS NULL OR PlayerID = @PlayerID)
                    ORDER BY AttemptNo";

                var results = await _db.QueryAsync<FieldEventsDTO.FieldEventResult, dynamic>(
                    sql,
                    new
                    {
                        EventID = eventId,
                        RegionID = regionId,
                        PlayerID = playerId
                    });

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching participant attempts for EventID {EventID}, RegionID {RegionID}", eventId, regionId);
                return StatusCode(500, $"Error retrieving attempts: {ex.Message}");
            }
        }

        // Clear all results for an event
        [HttpDelete("ClearResults/{eventId}")]
        public async Task<IActionResult> ClearFieldEventResults(string eventId)
        {
            try
            {
                await _db.ExecuteAsync<dynamic>(
                    "DELETE FROM FieldEvents WHERE EventID = @EventID",
                    new { EventID = eventId });

                return Ok(new { message = "All results cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing results for EventID {EventID}", eventId);
                return StatusCode(500, $"Error clearing results: {ex.Message}");
            }
        }
    }
}