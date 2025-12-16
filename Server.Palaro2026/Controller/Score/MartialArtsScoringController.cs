using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.Services;
using Server.Palaro2026.DTO;
using System.Data;

namespace Server.Palaro2026.Controller.Score
{
    [Route("api/[controller]")]
    [ApiController]
    public class MartialArtsScoringController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<MartialArtsScoringController> _logger;

        public MartialArtsScoringController(ISqlDataAccess db, ILogger<MartialArtsScoringController> logger)
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

        // GET all martial arts records
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MartialArtsScoringDTO>>> GetMartialArtsScoring()
        {
            try
            {
                string sql = @"
            SELECT 
                mas.ID,
                mas.EventID,
                mas.EventVersusID,
                mas.RegionID,
                mas.GameNo,
                mas.MatchId,
                mas.MatchPosition,
                mas.SetNo,
                mas.Result,
                mas.CreatedAt,
                mas.UpdatedAt,
                sr.Region,
                sr.Abbreviation,
                e.Date,
                e.Time,
                s.Sport,
                ssc.Subcategory,
                sgc.Gender,
                sl.Level,
                es.Stage as EventStage,
                mas.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM MartialArtsScoring mas
            LEFT JOIN SchoolRegions sr ON mas.RegionID = sr.ID
            LEFT JOIN Events e ON mas.EventID = e.ID
            LEFT JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
            LEFT JOIN Sports s ON ssc.SportID = s.ID
            LEFT JOIN SportGenderCategories sgc ON ssc.SportGenderCategoryID = sgc.ID
            LEFT JOIN SchoolLevels sl ON ssc.SchoolLevelID = sl.ID
            LEFT JOIN EventStages es ON e.EventStageID = es.ID
            LEFT JOIN ProfilePlayers pp ON mas.PlayerID = pp.ID
            ORDER BY mas.GameNo, mas.MatchId, mas.SetNo, mas.MatchPosition";

                var data = await _db.QueryAsync<MartialArtsScoringDTO, dynamic>(sql, new { });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching martial arts scoring data");
                return StatusCode(500, $"Error retrieving martial arts scoring data: {ex.Message}");
            }
        }

        // GET martial arts by event ID
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<MartialArtsScoringDTO>>> GetMartialArtsScoringByEvent(string eventId)
        {
            try
            {
                if (!await EventExists(eventId))
                    return NotFound($"Event with ID '{eventId}' not found.");

                string sql = @"
            SELECT 
                mas.ID,
                mas.EventID,
                mas.EventVersusID,
                mas.RegionID,
                mas.GameNo,
                mas.MatchId,
                mas.MatchPosition,
                mas.SetNo,
                mas.Result,
                mas.CreatedAt,
                mas.UpdatedAt,
                sr.Region,
                sr.Abbreviation,
                mas.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM MartialArtsScoring mas
            LEFT JOIN SchoolRegions sr ON mas.RegionID = sr.ID
            LEFT JOIN ProfilePlayers pp ON mas.PlayerID = pp.ID
            WHERE mas.EventID = @EventID
            ORDER BY mas.GameNo, mas.MatchId, mas.SetNo, mas.MatchPosition";

                var data = await _db.QueryAsync<MartialArtsScoringDTO, dynamic>(sql, new { EventID = eventId });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching martial arts scoring data for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving martial arts scoring data: {ex.Message}");
            }
        }

        // GET martial arts events with filters
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<MartialArtsEventDTO>>> GetMartialArtsEvents(
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
                        WHERE s.Sport IN ('Arnis', 'Boxing', 'Taekwondo', 'Wrestling', 'Wushu', 'Pencak Silat')";

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

                sql += " ORDER BY e.Date, e.Time";

                var events = await _db.QueryAsync<MartialArtsEventDTO, dynamic>(sql, parameters);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching martial arts events");
                return StatusCode(500, $"Error retrieving martial arts events: {ex.Message}");
            }
        }

        // POST martial arts scoring record
        [HttpPost]
        public async Task<ActionResult<int>> CreateMartialArtsScoring([FromBody] CreateMartialArtsScoringDTO request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.EventID) && !await EventExists(request.EventID))
                    return NotFound($"Event with ID '{request.EventID}' not found.");

                string sql = @"INSERT INTO MartialArtsScoring (
                            EventID, EventVersusID, RegionID, GameNo, MatchId, 
                            MatchPosition, SetNo, Result, PlayerID, CreatedAt, UpdatedAt) 
                            OUTPUT INSERTED.ID
                            VALUES (
                                @EventID, @EventVersusID, @RegionID, @GameNo, @MatchId,
                                @MatchPosition, @SetNo, @Result, @PlayerID, GETDATE(), GETDATE()
                            )";

                var id = await _db.ExecuteScalarAsync<int, dynamic>(sql, new
                {
                    request.EventID,
                    request.EventVersusID,
                    request.RegionID,
                    request.GameNo,
                    request.MatchId,
                    request.MatchPosition,
                    request.SetNo,
                    request.Result,
                    request.PlayerID
                });

                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating martial arts scoring record");
                return StatusCode(500, $"Error creating martial arts scoring record: {ex.Message}");
            }
        }

        // GET martial arts standings by event ID
        [HttpGet("event/{eventId}/standings")]
        public async Task<ActionResult<IEnumerable<object>>> GetMartialArtsStandingsByEvent(string eventId)
        {
            try
            {
                if (!await EventExists(eventId))
                    return NotFound($"Event with ID '{eventId}' not found.");

                string sql = @"
            SELECT 
                sr.Abbreviation as RegionName,
                sr.ID as RegionId,
                COUNT(DISTINCT mas.MatchId) as MatchesPlayed,
                SUM(CASE WHEN mas.Result = 'W' THEN 1 ELSE 0 END) as SetsWon,
                SUM(CASE WHEN mas.Result = 'L' THEN 1 ELSE 0 END) as SetsLost,
                mas.PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM MartialArtsScoring mas
            LEFT JOIN SchoolRegions sr ON mas.RegionID = sr.ID
            LEFT JOIN ProfilePlayers pp ON mas.PlayerID = pp.ID
            WHERE mas.EventID = @EventID
            GROUP BY sr.Abbreviation, sr.ID, mas.PlayerID, pp.FirstName, pp.LastName
            ORDER BY SetsWon DESC, SetsLost ASC";

                var data = await _db.QueryAsync<dynamic, dynamic>(sql, new { EventID = eventId });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching martial arts standings for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving martial arts standings: {ex.Message}");
            }
        }

        // Update martial arts scoring record
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMartialArtsScoring(int id, [FromBody] UpdateMartialArtsScoringDTO request)
        {
            try
            {
                string getSql = "SELECT * FROM MartialArtsScoring WHERE ID = @ID";
                var existingRecord = await _db.QueryFirstAsync<MartialArtsScoringDTO, dynamic>(getSql, new { ID = id });

                if (existingRecord == null)
                    return NotFound($"Martial arts scoring record with ID {id} not found");

                string sql = @"UPDATE MartialArtsScoring 
                                SET GameNo = @GameNo,
                                    MatchId = @MatchId,
                                    MatchPosition = @MatchPosition,
                                    SetNo = @SetNo,
                                    Result = @Result,
                                    PlayerID = @PlayerID,
                                    UpdatedAt = GETDATE()
                                WHERE ID = @ID";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    GameNo = request.GameNo != 0 ? request.GameNo : existingRecord.GameNo,
                    MatchId = request.MatchId != 0 ? request.MatchId : existingRecord.MatchId,
                    MatchPosition = !string.IsNullOrEmpty(request.MatchPosition) ? request.MatchPosition : existingRecord.MatchPosition,
                    SetNo = request.SetNo != 0 ? request.SetNo : existingRecord.SetNo,
                    Result = !string.IsNullOrEmpty(request.Result) ? request.Result : existingRecord.Result,
                    PlayerID = !string.IsNullOrEmpty(request.PlayerID) ? request.PlayerID : existingRecord.PlayerID,
                    ID = id
                });

                return Ok(new { message = "Martial arts scoring record updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating martial arts scoring record with ID {ID}", id);
                return StatusCode(500, $"Error updating martial arts scoring record: {ex.Message}");
            }
        }

        // Delete martial arts scoring record
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMartialArtsScoring(int id)
        {
            try
            {
                string sql = "DELETE FROM MartialArtsScoring WHERE ID = @ID";
                await _db.ExecuteAsync<dynamic>(sql, new { ID = id });

                return Ok(new { message = "Martial arts scoring record deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting martial arts scoring record with ID {ID}", id);
                return StatusCode(500, $"Error deleting martial arts scoring record: {ex.Message}");
            }
        }

        // Delete all martial arts scoring for a region in all matches
        [HttpDelete("region/{regionId}/match/all")]
        public async Task<IActionResult> DeleteMartialArtsScoringByRegion(int regionId)
        {
            try
            {
                string sql = "DELETE FROM MartialArtsScoring WHERE RegionID = @RegionID";
                await _db.ExecuteAsync<dynamic>(sql, new { RegionID = regionId });

                return Ok(new { message = "Martial arts scoring records deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting martial arts scoring records for RegionID {RegionID}", regionId);
                return StatusCode(500, $"Error deleting martial arts scoring records: {ex.Message}");
            }
        }

        // Delete martial arts scoring for specific match and set
        [HttpDelete("match/{matchId}/set/{setNo}")]
        public async Task<IActionResult> DeleteMartialArtsScoringByMatchAndSet(int matchId, int setNo)
        {
            try
            {
                string sql = "DELETE FROM MartialArtsScoring WHERE MatchId = @MatchId AND SetNo = @SetNo";
                await _db.ExecuteAsync<dynamic>(sql, new { MatchId = matchId, SetNo = setNo });

                return Ok(new { message = "Martial arts scoring records deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting martial arts scoring records for MatchId {MatchId} and SetNo {SetNo}", matchId, setNo);
                return StatusCode(500, $"Error deleting martial arts scoring records: {ex.Message}");
            }
        }

        // Delete martial arts scoring by game number
        [HttpDelete("game/{gameNo}")]
        public async Task<IActionResult> DeleteMartialArtsScoringByGame(int gameNo)
        {
            try
            {
                string sql = "DELETE FROM MartialArtsScoring WHERE GameNo = @GameNo";
                await _db.ExecuteAsync<dynamic>(sql, new { GameNo = gameNo });

                return Ok(new { message = $"Martial arts scoring records for Game {gameNo} deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting martial arts scoring records for GameNo {GameNo}", gameNo);
                return StatusCode(500, $"Error deleting martial arts scoring records: {ex.Message}");
            }
        }

        // Delete martial arts scoring for a specific region in a specific match
        [HttpDelete("region/{regionId}/match/{matchId}")]
        public async Task<IActionResult> DeleteMartialArtsScoringByRegionAndMatch(int regionId, int matchId)
        {
            try
            {
                string sql = "DELETE FROM MartialArtsScoring WHERE RegionID = @RegionID AND MatchId = @MatchId";
                await _db.ExecuteAsync<dynamic>(sql, new { RegionID = regionId, MatchId = matchId });

                return Ok(new { message = "Martial arts scoring records deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting martial arts scoring records for RegionID {RegionID} and MatchId {MatchId}", regionId, matchId);
                return StatusCode(500, $"Error deleting martial arts scoring records: {ex.Message}");
            }
        }
    }
}