using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;
using System.Data;

namespace Server.Palaro2026.Controller.Score
{
    [Route("api/score/[controller]")]
    [ApiController]
    public class ArcheryScoringController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<ArcheryScoringController> _logger;

        public ArcheryScoringController(ISqlDataAccess db, ILogger<ArcheryScoringController> logger)
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

        // GET by Event ID
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<ArcheryScoringDTO>>> GetByEvent(string eventId, [FromQuery] int? roundNo = null, [FromQuery] int? endNo = null)
        {
            try
            {
                if (!await EventExists(eventId))
                    return NotFound($"Event with ID '{eventId}' not found.");

                string sql = @"
                                SELECT 
                                    a.ID,
                                    a.EventID,
                                    a.EventVersusID,
                                    a.RoundNo,
                                    a.ShotNo,
                                    a.ShotScore,
                                    a.IsBullseye,
                                    a.IsWinner,
                                    a.RegionID,
                                    a.PlayerID,
                                    a.Lane,
                                    a.EndNo,
                                    a.Position,
                                    a.CreatedAt,
                                    a.UpdatedAt,
                                    sr.Region,
                                    sr.Abbreviation,
                                    pp.FirstName + ' ' + pp.LastName as PlayerName
                                FROM ArcheryScoring a
                                LEFT JOIN SchoolRegions sr ON a.RegionID = sr.ID
                                LEFT JOIN ProfilePlayers pp ON a.PlayerID = pp.ID
                                WHERE a.EventID = @EventID
                                AND (@RoundNo IS NULL OR a.RoundNo = @RoundNo)
                                AND (@EndNo IS NULL OR a.EndNo = @EndNo)
                                ORDER BY a.RoundNo, a.Lane, a.EndNo, a.ShotNo";

                var data = await _db.QueryAsync<ArcheryScoringDTO, dynamic>(sql, new { EventID = eventId, RoundNo = roundNo, EndNo = endNo });

                _logger.LogInformation("GetByEvent: EventID={EventID}, RoundNo={RoundNo}, EndNo={EndNo}, Returned {Count} records",
                    eventId, roundNo, endNo, data?.Count() ?? 0);

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ArcheryScoring data for EventID {EventID}", eventId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("lane/{lane}/round/{roundNo}/end/{endNo}")]
        public async Task<ActionResult<IEnumerable<ArcheryScoringDTO>>> GetByLaneRoundAndEnd(int lane, int roundNo, int endNo, [FromQuery] string? eventId = null, [FromQuery] string? playerId = null)
        {
            try
            {
                string sql = @"
            SELECT 
                a.ID,
                a.EventID,
                a.EventVersusID,
                a.RoundNo,
                a.ShotNo,
                a.ShotScore,
                a.IsBullseye,
                a.IsWinner,
                a.RegionID,
                a.PlayerID,
                a.Lane,
                a.EndNo,
                a.Position,
                a.CreatedAt,
                a.UpdatedAt,
                sr.Region,
                sr.Abbreviation,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM ArcheryScoring a
            LEFT JOIN SchoolRegions sr ON a.RegionID = sr.ID
            LEFT JOIN ProfilePlayers pp ON a.PlayerID = pp.ID
            WHERE a.Lane = @Lane 
            AND a.RoundNo = @RoundNo
            AND a.EndNo = @EndNo
            AND (@EventID IS NULL OR a.EventID = @EventID)
            AND (@PlayerID IS NULL OR a.PlayerID = @PlayerID)
            ORDER BY a.ShotNo";

                var data = await _db.QueryAsync<ArcheryScoringDTO, dynamic>(sql, new
                {
                    Lane = lane,
                    RoundNo = roundNo,
                    EndNo = endNo,
                    EventID = eventId,
                    PlayerID = playerId
                });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ArcheryScoring records for Lane {Lane}, Round {RoundNo}, End {EndNo}", lane, roundNo, endNo);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET archery events with filters
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<ArcheryEventDTO>>> GetArcheryEvents(
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
                e.OnStream,
                e.IsFinished,
                e.SportMainCat,
                e.Archived,
                e.Deleted,
                sr.Region,
                sr.Abbreviation,
                sr.ID as RegionID,
                evtp.ProfilePlayerID as PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName,
                -- Determine if this is an individual event
                CASE 
                    WHEN ssc.Subcategory LIKE '%individual%' OR 
                         (ssc.Subcategory NOT LIKE '%team%' AND ssc.Subcategory NOT LIKE '%mixed%') 
                    THEN 1 
                    ELSE 0 
                END as IsIndividual
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
            WHERE s.Sport = 'Archery'";

                var conditions = new List<string>();
                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(eventId))
                {
                    conditions.Add("e.ID = @EventId");
                    parameters.Add("EventId", eventId);
                }
                else
                {
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

                var events = await _db.QueryAsync<ArcheryEventDTO, dynamic>(sql, parameters);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching archery events");
                return StatusCode(500, $"Error retrieving archery events: {ex.Message}");
            }
        }

        // POST create new 1440 scoring record
        [HttpPost("1440")]
        public async Task<ActionResult<int>> Create1440([FromBody] CreateArcheryScoringDTO data)
        {
            try
            {
                if (data == null)
                    return BadRequest("Invalid archery scoring data.");

                if (!string.IsNullOrEmpty(data.EventID) && !await EventExists(data.EventID))
                    return NotFound($"Event with ID '{data.EventID}' not found.");

                string checkSql = @"
        SELECT COUNT(1) 
        FROM ArcheryScoring 
        WHERE EventID = @EventID 
        AND RegionID = @RegionID 
        AND RoundNo = @RoundNo 
        AND ShotNo = @ShotNo
        AND Lane = @Lane
        AND EndNo = @EndNo
        AND PlayerID = @PlayerID";

                var existingCount = await _db.ExecuteScalarAsync<int, dynamic>(checkSql, new
                {
                    data.EventID,
                    data.RegionID,
                    data.RoundNo,
                    data.ShotNo,
                    data.Lane,
                    data.EndNo,
                    data.PlayerID
                });

                if (existingCount > 0)
                {
                    return Conflict("Shot already recorded for this combination.");
                }

                string sql = @"
        INSERT INTO ArcheryScoring (
            EventID, EventVersusID, RoundNo, ShotNo, ShotScore, 
            IsBullseye, IsWinner, RegionID, PlayerID, Lane, EndNo, CreatedAt, UpdatedAt
        )
        OUTPUT INSERTED.ID
        VALUES (
            @EventID, @EventVersusID, @RoundNo, @ShotNo, @ShotScore,
            @IsBullseye, @IsWinner, @RegionID, @PlayerID, @Lane, @EndNo, GETDATE(), GETDATE()
        )";

                int newId = await _db.ExecuteScalarAsync<int, dynamic>(sql, new
                {
                    data.EventID,
                    data.EventVersusID,
                    data.RoundNo,
                    data.ShotNo,
                    data.ShotScore,
                    data.IsBullseye,
                    data.IsWinner,
                    data.RegionID,
                    data.PlayerID,
                    data.Lane,
                    data.EndNo
                });

                if (newId > 0)
                    return Ok(newId);

                return BadRequest("Failed to insert ArcheryScoring record.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ArcheryScoring record");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST create Olympic round scoring record
        [HttpPost("olympic")]
        public async Task<ActionResult<int>> CreateOlympic([FromBody] CreateArcheryScoringDTO data)
        {
            try
            {
                if (data == null)
                    return BadRequest("Invalid archery scoring data.");

                if (!string.IsNullOrEmpty(data.EventID) && !await EventExists(data.EventID))
                    return NotFound($"Event with ID '{data.EventID}' not found.");

                if (!string.IsNullOrEmpty(data.PlayerID))
                {
                    string verifyPlayerSql = @"
                SELECT COUNT(1) 
                FROM EventVersusTeamPlayers evtp
                INNER JOIN EventVersusTeams evt ON evtp.EventVersusID = evt.ID
                WHERE evt.EventID = @EventID
                AND evt.SchoolRegionID = @RegionID
                AND evtp.ProfilePlayerID = @PlayerID";

                    var playerExists = await _db.ExecuteScalarAsync<int, dynamic>(verifyPlayerSql, new
                    {
                        data.EventID,
                        data.RegionID,
                        data.PlayerID
                    });

                    if (playerExists == 0)
                    {
                        _logger.LogWarning("Player {PlayerID} does not belong to Region {RegionID} in Event {EventID}",
                            data.PlayerID, data.RegionID, data.EventID);
                        return BadRequest($"Player {data.PlayerID} is not part of this team/region in this event.");
                    }
                }

                string checkSql = @"
            SELECT COUNT(1) 
            FROM ArcheryScoring 
            WHERE EventID = @EventID 
            AND RegionID = @RegionID 
            AND RoundNo = @RoundNo 
            AND ShotNo = @ShotNo
            AND Lane = @Lane
            AND EndNo = @EndNo
            AND PlayerID = @PlayerID
            AND Position = @Position";

                var existingCount = await _db.ExecuteScalarAsync<int, dynamic>(checkSql, new
                {
                    data.EventID,
                    data.RegionID,
                    data.RoundNo,
                    data.ShotNo,
                    data.Lane,
                    data.EndNo,
                    data.PlayerID,
                    data.Position
                });

                if (existingCount > 0)
                {
                    return Conflict("Shot already recorded for this combination.");
                }

                string sql = @"
            INSERT INTO ArcheryScoring (
                EventID, EventVersusID, RoundNo, ShotNo, ShotScore, 
                IsBullseye, IsWinner, RegionID, PlayerID, Lane, EndNo, 
                Position, CreatedAt, UpdatedAt
            )
            OUTPUT INSERTED.ID
            VALUES (
                @EventID, @EventVersusID, @RoundNo, @ShotNo, @ShotScore,
                @IsBullseye, @IsWinner, @RegionID, @PlayerID, @Lane, @EndNo,
                @Position, GETDATE(), GETDATE()
            )";

                int newId = await _db.ExecuteScalarAsync<int, dynamic>(sql, new
                {
                    data.EventID,
                    data.EventVersusID,
                    data.RoundNo,
                    data.ShotNo,
                    data.ShotScore,
                    data.IsBullseye,
                    data.IsWinner,
                    data.RegionID,
                    data.PlayerID,
                    data.Lane,
                    data.EndNo,
                    data.Position
                });

                if (newId > 0)
                    return Ok(newId);

                return BadRequest("Failed to insert ArcheryScoring record.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Olympic ArcheryScoring record");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArcheryScoringDTO data)
        {
            try
            {
                if (data == null)
                    return BadRequest("Invalid update data.");

                string getSql = "SELECT * FROM ArcheryScoring WHERE ID = @ID";
                var existingRecord = await _db.QueryAsync<ArcheryScoringDTO, dynamic>(getSql, new { ID = id });

                if (existingRecord == null)
                    return NotFound($"Archery scoring record with ID {id} not found");

                string sql = @"
            UPDATE ArcheryScoring
            SET 
                RoundNo = ISNULL(@RoundNo, RoundNo),
                ShotNo = ISNULL(@ShotNo, ShotNo),
                ShotScore = ISNULL(@ShotScore, ShotScore),
                IsBullseye = ISNULL(@IsBullseye, IsBullseye),
                IsWinner = ISNULL(@IsWinner, IsWinner),
                RegionID = ISNULL(@RegionID, RegionID),
                Lane = ISNULL(@Lane, Lane),
                EndNo = ISNULL(@EndNo, EndNo),
                Position = ISNULL(@Position, Position),
                UpdatedAt = GETDATE()
            WHERE ID = @ID";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    ID = id,
                    data.RoundNo,
                    data.ShotNo,
                    data.ShotScore,
                    data.IsBullseye,
                    data.IsWinner,
                    data.RegionID,
                    data.Lane,
                    data.EndNo,
                    data.Position 
                });

                return Ok("Archery scoring record updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ArcheryScoring with ID {ID}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE an archery scoring record
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string sql = "DELETE FROM ArcheryScoring WHERE ID = @ID";
                await _db.ExecuteAsync<dynamic>(sql, new { ID = id });

                return Ok("Archery scoring record deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ArcheryScoring with ID {ID}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE all shots for a region in a specific round, lane and end
        [HttpDelete("region/{regionId}/round/{roundNo}/lane/{lane}/end/{endNo}")]
        public async Task<IActionResult> DeleteByRegionRoundLaneAndEnd(int regionId, int roundNo, int lane, int endNo, [FromQuery] string? position = null)
        {
            try
            {
                string sql = @"
            DELETE FROM ArcheryScoring 
            WHERE RegionID = @RegionID 
            AND RoundNo = @RoundNo 
            AND Lane = @Lane
            AND EndNo = @EndNo
            AND (@Position IS NULL OR Position = @Position)";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    RegionID = regionId,
                    RoundNo = roundNo,
                    Lane = lane,
                    EndNo = endNo,
                    Position = position
                });

                return Ok("Archery scoring records deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ArcheryScoring records for RegionID {RegionID}, RoundNo {RoundNo}, Lane {Lane}, EndNo {EndNo}", regionId, roundNo, lane, endNo);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE all shots for a region in a specific round and lane
        [HttpDelete("region/{regionId}/round/{roundNo}/lane/{lane}")]
        public async Task<IActionResult> DeleteByRegionRoundAndLane(int regionId, int roundNo, int lane)
        {
            try
            {
                string sql = @"
                    DELETE FROM ArcheryScoring 
                    WHERE RegionID = @RegionID 
                    AND RoundNo = @RoundNo 
                    AND Lane = @Lane";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    RegionID = regionId,
                    RoundNo = roundNo,
                    Lane = lane
                });

                return Ok("Archery scoring records deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ArcheryScoring records for RegionID {RegionID}, RoundNo {RoundNo}, Lane {Lane}", regionId, roundNo, lane);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE all shots for a specific match
        [HttpDelete("olympic/match/{lane}/round/{roundNo}")]
        public async Task<IActionResult> DeleteOlympicMatchShots(int lane, int roundNo, [FromQuery] string? eventId = null)
        {
            try
            {
                string sql = @"
            DELETE FROM ArcheryScoring 
            WHERE Lane = @Lane 
            AND RoundNo = @RoundNo 
            AND (@EventID IS NULL OR EventID = @EventID)"; 

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    Lane = lane,
                    RoundNo = roundNo,
                    EventID = eventId
                });

                return Ok("Olympic match shots deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Olympic shots for Lane {Lane}, RoundNo {RoundNo}", lane, roundNo);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}

