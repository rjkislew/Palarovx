using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.Services;
using Server.Palaro2026.DTO;
using System.Data;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BilliardsScoringController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<BilliardsScoringController> _logger;

        public BilliardsScoringController(ISqlDataAccess db, ILogger<BilliardsScoringController> logger)
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

        // GET all billiards records
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BilliardsScoringDTO>>> GetBilliardsScoring()
        {
            try
            {
                string sql = @"
            SELECT 
                bs.ID,
                bs.EventID,
                bs.EventVersusID,
                bs.RegionID,
                bs.SetNo,
                bs.TableNo,
                bs.PlayerPosition,
                bs.Score,
                bs.IsWinner,
                bs.CreatedAt,
                bs.UpdatedAt,
                sr.Region,
                sr.Abbreviation,
                e.Date,
                e.Time,
                s.Sport,
                ssc.Subcategory,
                sgc.Gender,
                sl.Level,
                es.Stage as EventStage,
                bs.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM BilliardsScoring bs
            LEFT JOIN SchoolRegions sr ON bs.RegionID = sr.ID
            LEFT JOIN Events e ON bs.EventID = e.ID
            LEFT JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
            LEFT JOIN Sports s ON ssc.SportID = s.ID
            LEFT JOIN SportGenderCategories sgc ON ssc.SportGenderCategoryID = sgc.ID
            LEFT JOIN SchoolLevels sl ON ssc.SchoolLevelID = sl.ID
            LEFT JOIN EventStages es ON e.EventStageID = es.ID
            LEFT JOIN ProfilePlayers pp ON bs.PlayerID = pp.ID
            ORDER BY bs.SetNo, bs.TableNo, bs.PlayerPosition";

                var data = await _db.QueryAsync<BilliardsScoringDTO, dynamic>(sql, new { });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billiards scoring data");
                return StatusCode(500, $"Error retrieving billiards scoring data: {ex.Message}");
            }
        }

        // GET billiards by event ID
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<BilliardsScoringDTO>>> GetBilliardsScoringByEvent(string eventId)
        {
            try
            {
                if (!await EventExists(eventId))
                    return NotFound($"Event with ID '{eventId}' not found.");

                string sql = @"
            SELECT 
                bs.ID,
                bs.EventID,
                bs.EventVersusID,
                bs.RegionID,
                bs.SetNo,
                bs.TableNo,
                bs.PlayerPosition,
                bs.Score, -- FIXED: Added comma here
                bs.IsWinner,
                bs.CreatedAt,
                bs.UpdatedAt,
                sr.Region,
                sr.Abbreviation,
                bs.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM BilliardsScoring bs
            LEFT JOIN SchoolRegions sr ON bs.RegionID = sr.ID
            LEFT JOIN ProfilePlayers pp ON bs.PlayerID = pp.ID
            WHERE bs.EventID = @EventID
            ORDER BY bs.SetNo, bs.TableNo, bs.PlayerPosition";

                var data = await _db.QueryAsync<BilliardsScoringDTO, dynamic>(sql, new { EventID = eventId });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billiards scoring data for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving billiards scoring data: {ex.Message}");
            }
        }

        // GET billiards events with filters
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<BilliardsEventDTO>>> GetBilliardsEvents(
            [FromQuery] string? sport = null,
            [FromQuery] string? subcategory = null,
            [FromQuery] string? gender = null,
            [FromQuery] string? level = null,
            [FromQuery] string? eventStage = null,
            [FromQuery] string? eventId = null) // Add eventId parameter
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
            WHERE s.Sport = 'Billiards'";

                var conditions = new List<string>();
                var parameters = new DynamicParameters();

                // Filter by eventId if provided
                if (!string.IsNullOrEmpty(eventId))
                {
                    conditions.Add("e.ID = @EventID");
                    parameters.Add("EventID", eventId);
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

                sql += " ORDER BY e.Date, e.Time";

                var events = await _db.QueryAsync<BilliardsEventDTO, dynamic>(sql, parameters);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billiards events");
                return StatusCode(500, $"Error retrieving billiards events: {ex.Message}");
            }
        }

        // POST create billiards scoring record
        [HttpPost]
        public async Task<ActionResult<int>> CreateBilliardsScoring([FromBody] CreateBilliardsScoringDTO request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.EventID) && !await EventExists(request.EventID))
                    return NotFound($"Event with ID '{request.EventID}' not found.");

                // Check if record already exists
                string checkSql = @"
            SELECT ID FROM BilliardsScoring 
            WHERE EventID = @EventID 
            AND RegionID = @RegionID 
            AND SetNo = @SetNo 
            AND TableNo = @TableNo 
            AND PlayerPosition = @PlayerPosition";

                var existingId = await _db.ExecuteScalarAsync<int?, dynamic>(checkSql, new
                {
                    request.EventID,
                    request.RegionID,
                    request.SetNo,
                    request.TableNo,
                    request.PlayerPosition
                });

                // If record exists, update it instead of creating new
                if (existingId.HasValue && existingId.Value > 0)
                {
                    string updateSql = @"
                UPDATE BilliardsScoring 
                SET Score = @Score,
                    IsWinner = @IsWinner,
                    PlayerID = @PlayerID,
                    UpdatedAt = GETDATE()
                WHERE ID = @ID";

                    await _db.ExecuteAsync<dynamic>(updateSql, new
                    {
                        request.Score,
                        request.IsWinner,
                        request.PlayerID,
                        ID = existingId.Value
                    });

                    return Ok(existingId.Value);
                }

                // Create new record only if doesn't exist
                string insertSql = @"
            INSERT INTO BilliardsScoring (
                EventID, EventVersusID, RegionID, SetNo, 
                TableNo, PlayerPosition, Score, IsWinner, PlayerID, CreatedAt, UpdatedAt
            ) 
            OUTPUT INSERTED.ID
            VALUES (
                @EventID, @EventVersusID, @RegionID, @SetNo,
                @TableNo, @PlayerPosition, @Score, @IsWinner, @PlayerID, GETDATE(), GETDATE()
            )";

                var id = await _db.ExecuteScalarAsync<int, dynamic>(insertSql, new
                {
                    request.EventID,
                    request.EventVersusID,
                    request.RegionID,
                    request.SetNo,
                    request.TableNo,
                    request.PlayerPosition,
                    request.Score,
                    request.IsWinner,
                    request.PlayerID
                });

                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating billiards scoring record");
                return StatusCode(500, $"Error creating billiards scoring record: {ex.Message}");
            }
        }

        // GET billiards tournament bracket by event ID
        [HttpGet("event/{eventId}/bracket")]
        public async Task<ActionResult<IEnumerable<object>>> GetBilliardsBracketByEvent(string eventId)
        {
            try
            {
                if (!await EventExists(eventId))
                    return NotFound($"Event with ID '{eventId}' not found.");

                string sql = @"
            SELECT 
                bs.SetNo as Round,
                bs.TableNo,
                bs.PlayerPosition,
                bs.Score, -- FIXED: Added comma here
                sr.Abbreviation as RegionName,
                bs.IsWinner,
                bs.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM BilliardsScoring bs
            LEFT JOIN SchoolRegions sr ON bs.RegionID = sr.ID
            LEFT JOIN ProfilePlayers pp ON bs.PlayerID = pp.ID
            WHERE bs.EventID = @EventID
            ORDER BY bs.SetNo, bs.TableNo, bs.PlayerPosition";

                var data = await _db.QueryAsync<dynamic, dynamic>(sql, new { EventID = eventId });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billiards bracket for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving billiards bracket: {ex.Message}");
            }
        }

        // Update billiards scoring record
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBilliardsScoring(int id, [FromBody] UpdateBilliardsScoringDTO request)
        {
            try
            {
                string getSql = "SELECT * FROM BilliardsScoring WHERE ID = @ID";
                var existingRecord = await _db.QueryFirstAsync<BilliardsScoringDTO, dynamic>(getSql, new { ID = id });

                if (existingRecord == null)
                    return NotFound($"Billiards scoring record with ID {id} not found");

                string sql = @"
            UPDATE BilliardsScoring 
            SET TableNo = @TableNo,
                PlayerPosition = @PlayerPosition,
                Score = @Score,
                IsWinner = @IsWinner,
                SetNo = @SetNo,
                PlayerID = @PlayerID,
                UpdatedAt = GETDATE()
            WHERE ID = @ID";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    TableNo = request.TableNo != 0 ? request.TableNo : existingRecord.TableNo,
                    PlayerPosition = !string.IsNullOrEmpty(request.PlayerPosition) ? request.PlayerPosition : existingRecord.PlayerPosition,
                    Score = request.Score.HasValue ? request.Score : existingRecord.Score,
                    IsWinner = request.IsWinner,
                    SetNo = request.SetNo != 0 ? request.SetNo : existingRecord.SetNo,
                    PlayerID = !string.IsNullOrEmpty(request.PlayerID) ? request.PlayerID : existingRecord.PlayerID,
                    ID = id
                });

                return Ok(new { message = "Billiards scoring record updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating billiards scoring record with ID {ID}", id);
                return StatusCode(500, $"Error updating billiards scoring record: {ex.Message}");
            }
        }

        // Delete billiards scoring record
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilliardsScoring(int id)
        {
            try
            {
                string sql = "DELETE FROM BilliardsScoring WHERE ID = @ID";
                await _db.ExecuteAsync<dynamic>(sql, new { ID = id });

                return Ok(new { message = "Billiards scoring record deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting billiards scoring record with ID {ID}", id);
                return StatusCode(500, $"Error deleting billiards scoring record: {ex.Message}");
            }
        }

        // Delete billiards scoring by region and set
        [HttpDelete("region/{regionId}/set/{setNo}")]
        public async Task<IActionResult> DeleteBilliardsScoringByRegionAndSet(int regionId, int setNo)
        {
            try
            {
                string sql = "DELETE FROM BilliardsScoring WHERE RegionID = @RegionID AND SetNo = @SetNo";
                await _db.ExecuteAsync<dynamic>(sql, new { RegionID = regionId, SetNo = setNo });

                return Ok(new { message = "Billiards scoring records deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting billiards scoring records for RegionID {RegionID} and SetNo {SetNo}", regionId, setNo);
                return StatusCode(500, $"Error deleting billiards scoring records: {ex.Message}");
            }
        }

        // Delete specific table assignment
        [HttpDelete("region/{regionId}/set/{setNo}/table/{tableNo}/position/{playerPosition}")]
        public async Task<IActionResult> DeleteBilliardsScoringByPosition(int regionId, int setNo, int tableNo, string playerPosition)
        {
            try
            {
                string sql = @"
            DELETE FROM BilliardsScoring 
            WHERE RegionID = @RegionID 
            AND SetNo = @SetNo 
            AND TableNo = @TableNo 
            AND PlayerPosition = @PlayerPosition";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    RegionID = regionId,
                    SetNo = setNo,
                    TableNo = tableNo,
                    PlayerPosition = playerPosition
                });

                return Ok(new { message = "Billiards scoring record deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting billiards scoring record for RegionID {RegionID}, SetNo {SetNo}, TableNo {TableNo}, Position {PlayerPosition}",
                    regionId, setNo, tableNo, playerPosition);
                return StatusCode(500, $"Error deleting billiards scoring record: {ex.Message}");
            }
        }
    }
}