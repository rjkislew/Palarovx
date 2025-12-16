using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;
using System.Data;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeDistanceEventsController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<TimeDistanceEventsController> _logger;

        public TimeDistanceEventsController(ISqlDataAccess db, ILogger<TimeDistanceEventsController> logger)
        {
            _db = db;
            _logger = logger;
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

        // POST athletics/swimming scoring
        [HttpPost("Sports/AthleticsSwimming")]
        public async Task<IActionResult> SaveAthleticsSwimmingScoring([FromBody] TimeDistanceEventsDTO.SaveAthleticsSwimmingRequest request)
        {
            try
            {
                string checkEventSql = "SELECT COUNT(1) FROM Events WHERE ID = @EventID";
                var eventCount = await _db.ExecuteScalarAsync<int, dynamic>(checkEventSql, new { EventID = request.EventID });
                if (eventCount == 0)
                    return NotFound($"Event with ID '{request.EventID}' not found.");

                await _db.ExecuteAsync<dynamic>(
                    "DELETE FROM TimeDistanceEvents WHERE EventID = @EventID",
                    new { EventID = request.EventID });

                foreach (var heat in request.Heats)
                {
                    foreach (var lane in heat.Lanes)
                    {
                        int laneNo = 1;
                        if (!string.IsNullOrEmpty(lane.LaneName))
                        {
                            var parts = lane.LaneName.Split(' ');
                            if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedLaneNo))
                                laneNo = parsedLaneNo;
                        }

                        string? playerId = null;

                        if (!string.IsNullOrEmpty(lane.PlayerID)) 
                        {
                            playerId = lane.PlayerID;
                            _logger.LogInformation("Using PlayerID {PlayerID} from request for RegionID {RegionID}",
                                playerId, lane.RegionID);
                        }
                        else if (lane.RegionID.HasValue)
                        {
                            playerId = await GetPlayerIdForRegion(request.EventID, lane.RegionID.Value);
                            _logger.LogInformation("Queried PlayerID {PlayerID} for RegionID {RegionID}",
                                playerId, lane.RegionID);
                        }

                        string insertSql = @"
                    INSERT INTO TimeDistanceEvents (EventID, RegionID, PlayerID, HeatNo, LaneNo, Result, Rank, CreatedAt, UpdatedAt)
                    VALUES (@EventID, @RegionID, @PlayerID, @HeatNo, @LaneNo, @Result, @Rank, GETDATE(), GETDATE())";

                        _logger.LogInformation("Inserting TimeDistanceEvent - EventID: {EventID}, RegionID: {RegionID}, PlayerID: {PlayerID}, HeatNo: {HeatNo}, LaneNo: {LaneNo}",
                            request.EventID, lane.RegionID, playerId, heat.HeatOrder, laneNo);

                        await _db.ExecuteAsync<dynamic>(
                            insertSql,
                            new
                            {
                                request.EventID,
                                RegionID = lane.RegionID ?? (object)DBNull.Value,
                                PlayerID = playerId ?? (object)DBNull.Value,
                                HeatNo = heat.HeatOrder,
                                LaneNo = laneNo,
                                Result = lane.Result ?? (object)DBNull.Value,
                                Rank = (object)DBNull.Value
                            });
                    }
                }

                return Ok(new { message = "Athletics/Swimming scoring data saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving athletics/swimming scoring for EventID {EventID}", request.EventID);
                return StatusCode(500, $"Error saving scoring data: {ex.Message}");
            }
        }

        // GET saved athletics/swimming scoring
        [HttpGet("Sports/AthleticsSwimming/{eventId}")]
        public async Task<ActionResult<dynamic>> GetAthleticsSwimmingScoring(string eventId)
        {
            try
            {
                string checkEventSql = "SELECT COUNT(1) FROM Events WHERE ID = @EventID";
                var eventCount = await _db.ExecuteScalarAsync<int, dynamic>(checkEventSql, new { EventID = eventId });
                if (eventCount == 0)
                    return NotFound($"Event with ID '{eventId}' not found.");

                var scoringSql = @"
                    SELECT 
                        tde.EventID,
                        tde.RegionID,
                        tde.PlayerID, 
                        tde.HeatNo,
                        tde.LaneNo,
                        tde.Result,
                        tde.Rank,
                        tde.CreatedAt,
                        tde.UpdatedAt,
                        sr.Abbreviation as RegionAbbreviation,
                        pp.FirstName,
                        pp.LastName
                    FROM TimeDistanceEvents tde
                    LEFT JOIN SchoolRegions sr ON tde.RegionID = sr.ID
                    LEFT JOIN ProfilePlayers pp ON tde.PlayerID = pp.ID
                    WHERE tde.EventID = @EventID
                    ORDER BY tde.HeatNo, tde.LaneNo";

                var scoringData = await _db.QueryAsync<dynamic, dynamic>(scoringSql, new { EventID = eventId });

                var heats = scoringData
                    .GroupBy(x => new { x.HeatNo })
                    .Select(g => new
                    {
                        HeatName = $"Heat {g.Key.HeatNo}",
                        HeatOrder = g.Key.HeatNo,
                        Lanes = g.Select(l => new
                        {
                            LaneName = $"Lane {l.LaneNo}",
                            LaneOrder = l.LaneNo,
                            Result = l.Result,
                            RegionID = l.RegionID,
                            PlayerID = l.PlayerID,
                            PlayerName = l.PlayerID != null && l.FirstName != null && l.LastName != null
                                ? $"{l.FirstName} {l.LastName}"
                                : string.Empty,
                            RegionAbbreviation = l.RegionAbbreviation ?? string.Empty
                        }).OrderBy(l => l.LaneOrder).ToList()
                    }).OrderBy(h => h.HeatOrder).ToList();

                var assignments = scoringData
                    .Where(x => x.RegionID != null)
                    .Select(x => new
                    {
                        RegionID = x.RegionID,
                        PlayerID = x.PlayerID,
                        PlayerName = x.PlayerID != null && x.FirstName != null && x.LastName != null
                            ? $"{x.FirstName} {x.LastName}"
                            : string.Empty,
                        RegionAbbreviation = x.RegionAbbreviation ?? string.Empty,
                        AssignedTable = $"Heat {x.HeatNo}-Lane {x.LaneNo}"
                    })
                    .Distinct()
                    .ToList();

                return Ok(new
                {
                    Heats = heats,
                    Assignments = assignments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching athletics/swimming scoring for EventID {EventID}", eventId);
                return StatusCode(500, $"Error retrieving scoring data: {ex.Message}");
            }
        }

        // GET Athletics and Swimming Events
        [HttpGet("Sports/AthleticsSwimming")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAthleticsAndSwimmingEvents(
            [FromQuery] string? sport = null,
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
                    WHERE s.Sport IN ('Athletics', 'Swimming')";

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

                sql += " ORDER BY e.Date, e.Time, s.Sport";

                var events = await _db.QueryAsync<dynamic, dynamic>(sql, parameters);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching athletics and swimming events");
                return StatusCode(500, $"Error retrieving events: {ex.Message}");
            }
        }

        // Player information for a specific region and event
        [HttpGet("PlayerInfo")]
        public async Task<ActionResult<dynamic>> GetPlayerInfo([FromQuery] string eventId, [FromQuery] int regionId)
        {
            try
            {
                string sql = @"
            SELECT 
                evtp.ProfilePlayerID as PlayerID,
                pp.FirstName + ' ' + pp.LastName as PlayerName
            FROM EventVersusTeams evt
            INNER JOIN EventVersusTeamPlayers evtp ON evt.ID = evtp.EventVersusID
            INNER JOIN ProfilePlayers pp ON evtp.ProfilePlayerID = pp.ID
            WHERE evt.EventID = @EventID AND evt.SchoolRegionID = @RegionId";

                var playerInfoList = await _db.QueryAsync<dynamic, dynamic>(sql, new
                {
                    EventID = eventId,
                    RegionId = regionId
                });

                var playerInfo = playerInfoList.FirstOrDefault();

                if (playerInfo != null)
                {
                    return Ok(new
                    {
                        PlayerID = playerInfo.PlayerID,
                        PlayerName = playerInfo.PlayerName
                    });
                }

                return NotFound($"No player found for EventID {eventId} and RegionID {regionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player info for EventID {EventID} and RegionID {RegionID}", eventId, regionId);
                return StatusCode(500, $"Error retrieving player info: {ex.Message}");
            }
        }
    }
}