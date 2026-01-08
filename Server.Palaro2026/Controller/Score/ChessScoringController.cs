using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.Services;
using Server.Palaro2026.DTO;
using System.Data;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChessScoringController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<ChessScoringController> _logger;

        public ChessScoringController(ISqlDataAccess db, ILogger<ChessScoringController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private async Task<bool> EventExists(string eventId)
        {
            try
            {
                string sql = "SELECT COUNT(1) FROM Events WHERE ID = @EventID";
                var count = await _db.ExecuteScalarAsync<int, dynamic>(sql, new { EventID = eventId });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if event exists: {EventID}", eventId);
                return false;
            }
        }

        // GET all chess records
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChessScoringDTO>>> GetChessScoring()
        {
            try
            {
                string sql = @"
            SELECT 
                cs.ID,
                cs.SportsID,
                cs.EventID,
                cs.EventVersusID,
                cs.RegionID,
                cs.SetNo,
                cs.TableNo,
                cs.ColorGroup,
                cs.Score,
                cs.CreatedAt,
                cs.UpdatedAt,
                sr.Region,
                sr.Abbreviation,
                e.Date,
                e.Time,
                s.Sport,
                ssc.Subcategory,
                sgc.Gender,
                sl.Level,
                es.Stage as EventStage,
                cs.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM EventChessScoring cs
            LEFT JOIN SchoolRegions sr ON cs.RegionID = sr.ID
            LEFT JOIN Events e ON cs.EventID = e.ID
            LEFT JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
            LEFT JOIN Sports s ON ssc.SportID = s.ID
            LEFT JOIN SportGenderCategories sgc ON ssc.SportGenderCategoryID = sgc.ID
            LEFT JOIN SchoolLevels sl ON ssc.SchoolLevelID = sl.ID
            LEFT JOIN EventStages es ON e.EventStageID = es.ID
            LEFT JOIN ProfilePlayers pp ON cs.PlayerID = pp.ID
            ORDER BY cs.SetNo, cs.TableNo, cs.ColorGroup";

                var data = await _db.QueryAsync<ChessScoringDTO, dynamic>(sql, new { });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chess scoring data");
                return StatusCode(500, $"Error retrieving chess scoring data: {ex.Message}");
            }
        }

        // GET chess by event ID
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<ChessScoringDTO>>> GetChessScoringByEvent(string eventId)
        {
            try
            {
                if (!await EventExists(eventId))
                    return NotFound($"Event with ID '{eventId}' not found.");

                string sql = @"
            SELECT 
                cs.ID,
                cs.SportsID,
                cs.EventID,
                cs.EventVersusID,
                cs.RegionID,
                cs.SetNo,
                cs.TableNo,
                cs.ColorGroup,
                cs.Score,
                cs.CreatedAt,
                cs.UpdatedAt,
                sr.Region,
                sr.Abbreviation,
                cs.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM EventChessScoring cs
            LEFT JOIN SchoolRegions sr ON cs.RegionID = sr.ID
            LEFT JOIN ProfilePlayers pp ON cs.PlayerID = pp.ID
            WHERE cs.EventID = @EventID
            ORDER BY cs.SetNo, cs.TableNo, cs.ColorGroup";

                var data = await _db.QueryAsync<ChessScoringDTO, dynamic>(sql, new { EventID = eventId });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chess scoring data for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving chess scoring data: {ex.Message}");
            }
        }

        // GET chess events with filters
        // UPDATE the GetChessEvents method in ChessScoringController.cs
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<ChessEventDTO>>> GetChessEvents(
            [FromQuery] string? eventId = null,
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
    WHERE s.Sport = 'Chess'";

                var conditions = new List<string>();
                var parameters = new DynamicParameters();

                // If eventId is provided, use it as the primary filter
                if (!string.IsNullOrEmpty(eventId))
                {
                    conditions.Add("e.ID = @EventId");
                    parameters.Add("EventId", eventId);
                }
                else
                {
                    // Use other filters only if eventId is not provided
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
                }

                if (conditions.Any())
                {
                    sql += " AND " + string.Join(" AND ", conditions);
                }

                sql += " ORDER BY e.Date, e.Time";

                var events = await _db.QueryAsync<ChessEventDTO, dynamic>(sql, parameters);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chess events");
                return StatusCode(500, $"Error retrieving chess events: {ex.Message}");
            }
        }

        // POST create chess scoring record
        [HttpPost]
        public async Task<ActionResult<int>> CreateChessScoring([FromBody] CreateChessScoringDTO request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.EventID) && !await EventExists(request.EventID))
                    return NotFound($"Event with ID '{request.EventID}' not found.");

                string sql = @"
            INSERT INTO EventChessScoring (
                SportsID, EventID, EventVersusID, RegionID, SetNo, 
                TableNo, ColorGroup, Score, PlayerID, CreatedAt, UpdatedAt
            ) 
            OUTPUT INSERTED.ID
            VALUES (
                @SportsID, @EventID, @EventVersusID, @RegionID, @SetNo,
                @TableNo, @ColorGroup, @Score, @PlayerID, GETDATE(), GETDATE()
            )";

                var id = await _db.ExecuteScalarAsync<int, dynamic>(sql, new
                {
                    request.SportsID,
                    request.EventID,
                    request.EventVersusID,
                    request.RegionID,
                    request.SetNo,
                    request.TableNo,
                    request.ColorGroup,
                    request.Score,
                    request.PlayerID
                });

                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chess scoring record");
                return StatusCode(500, $"Error creating chess scoring record: {ex.Message}");
            }
        }

        // GET chess standings by event ID
        [HttpGet("event/{eventId}/standings")]
        public async Task<ActionResult<IEnumerable<object>>> GetChessStandingsByEvent(string eventId)
        {
            try
            {
                if (!await EventExists(eventId))
                    return NotFound($"Event with ID '{eventId}' not found.");

                string sql = @"
            SELECT 
                sr.Abbreviation as RegionName,
                sr.ID as RegionId,
                SUM(cs.Score) as TotalPoints,
                COUNT(cs.ID) as GamesPlayed,
                SUM(CASE WHEN cs.Score = 1.0 THEN 1 ELSE 0 END) as Wins,
                SUM(CASE WHEN cs.Score = 0.5 THEN 1 ELSE 0 END) as Draws,
                SUM(CASE WHEN cs.Score = 0.0 THEN 1 ELSE 0 END) as Losses,
                COUNT(DISTINCT cs.SetNo) as SetsPlayed,
                cs.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM EventChessScoring cs
            LEFT JOIN SchoolRegions sr ON cs.RegionID = sr.ID
            LEFT JOIN ProfilePlayers pp ON cs.PlayerID = pp.ID
            WHERE cs.EventID = @EventID
            GROUP BY sr.Abbreviation, sr.ID, cs.PlayerID, pp.FirstName, pp.LastName
            ORDER BY TotalPoints DESC, Wins DESC, Losses ASC";

                var data = await _db.QueryAsync<dynamic, dynamic>(sql, new { EventID = eventId });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chess standings for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving chess standings: {ex.Message}");
            }
        }

        // update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChessScoring(int id, [FromBody] UpdateChessScoringDTO request)
        {
            try
            {
                string getSql = "SELECT * FROM EventChessScoring WHERE ID = @ID";
                var existingRecord = await _db.QueryFirstAsync<ChessScoringDTO, dynamic>(getSql, new { ID = id });

                if (existingRecord == null)
                    return NotFound($"Chess scoring record with ID {id} not found");

                string sql = @"
            UPDATE EventChessScoring 
            SET TableNo = @TableNo,
                ColorGroup = @ColorGroup,
                Score = @Score,
                SetNo = @SetNo,
                PlayerID = @PlayerID,
                UpdatedAt = GETDATE()
            WHERE ID = @ID";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    TableNo = request.TableNo != 0 ? request.TableNo : existingRecord.TableNo,
                    ColorGroup = !string.IsNullOrEmpty(request.ColorGroup) ? request.ColorGroup : existingRecord.ColorGroup,
                    Score = request.Score,
                    SetNo = request.SetNo != 0 ? request.SetNo : existingRecord.SetNo,
                    PlayerID = !string.IsNullOrEmpty(request.PlayerID) ? request.PlayerID : existingRecord.PlayerID,
                    ID = id
                });

                return Ok(new { message = "Chess scoring record updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chess scoring record with ID {ID}", id);
                return StatusCode(500, $"Error updating chess scoring record: {ex.Message}");
            }
        }

        // delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChessScoring(int id)
        {
            try
            {
                string sql = "DELETE FROM EventChessScoring WHERE ID = @ID";
                await _db.ExecuteAsync<dynamic>(sql, new { ID = id });

                return Ok(new { message = "Chess scoring record deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chess scoring record with ID {ID}", id);
                return StatusCode(500, $"Error deleting chess scoring record: {ex.Message}");
            }
        }

        // delete chess scoring by region and set
        [HttpDelete("region/{regionId}/set/{setNo}")]
        public async Task<IActionResult> DeleteChessScoringByRegionAndSet(int regionId, int setNo)
        {
            try
            {
                string sql = "DELETE FROM EventChessScoring WHERE RegionID = @RegionID AND SetNo = @SetNo";
                await _db.ExecuteAsync<dynamic>(sql, new { RegionID = regionId, SetNo = setNo });

                return Ok(new { message = "Chess scoring records deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chess scoring records for RegionID {RegionID} and SetNo {SetNo}", regionId, setNo);
                return StatusCode(500, $"Error deleting chess scoring records: {ex.Message}");
            }
        }

        private async Task<string?> GetPlayerIdForRegion(string eventId, int regionId)
        {
            try
            {
                string sql = @"
            SELECT evtp.ProfilePlayerID 
            FROM EventVersusTeamPlayers evtp
            INNER JOIN EventVersusTeams evt ON evtp.EventVersusID = evt.ID
            WHERE evt.EventID = @EventID AND evt.SchoolRegionID = @RegionID";

                var playerId = await _db.ExecuteScalarAsync<string?, dynamic>(sql, new
                {
                    EventID = eventId,
                    RegionID = regionId
                });

                _logger.LogInformation("Retrieved PlayerID {PlayerID} for EventID {EventID} and RegionID {RegionID}",
                    playerId, eventId, regionId);

                return playerId;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting player ID for EventID {EventID} and RegionID {RegionID}", eventId, regionId);
                return null;
            }
        }
    }

}