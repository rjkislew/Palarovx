using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;
using System.Data;

namespace Server.Palaro2026.Controller.Score
{
    [Route("api/score/[controller]")]
    [ApiController]
    public class SparringScoringController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<SparringScoringController> _logger;

        public SparringScoringController(ISqlDataAccess db, ILogger<SparringScoringController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET by EventID 
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<SparringDTO>>> GetByEvent(string eventId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        ss.ID,
                        ss.SportsID,
                        ss.EventID,
                        ss.EventVersusID,
                        ss.RegionID,
                        ss.RoundNo,
                        ss.MatNo,
                        ss.Corner,
                        ss.Result,
                        ss.PlayerID,
                        ss.CreatedAt,
                        ss.UpdatedAt,
                        sr.Region,
                        sr.Abbreviation,
                        s.Sport,
                        ssc.Subcategory,
                        sgc.Gender,
                        sl.Level,
                        es.Stage AS EventStage,
                        pp.FirstName + ' ' + pp.LastName AS PlayerName
                    FROM SparringScoring ss
                    LEFT JOIN Events e ON ss.EventID = e.ID
                    LEFT JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
                    LEFT JOIN Sports s ON ssc.SportID = s.ID
                    LEFT JOIN SportGenderCategories sgc ON ssc.SportGenderCategoryID = sgc.ID
                    LEFT JOIN SchoolLevels sl ON ssc.SchoolLevelID = sl.ID
                    LEFT JOIN EventStages es ON e.EventStageID = es.ID
                    LEFT JOIN SchoolRegions sr ON ss.RegionID = sr.ID
                    LEFT JOIN ProfilePlayers pp ON ss.PlayerID = pp.ID
                    WHERE ss.EventID = @EventID
                    ORDER BY ss.RoundNo, ss.MatNo, ss.Corner;
                ";

                var data = await _db.QueryAsync<SparringDTO, dynamic>(sql, new { EventID = eventId });

                if (data == null || !data.Any())
                    return NotFound($"No SparringScoring records found for EventID {eventId}");

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SparringScoring records for EventID {EventID}", eventId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET sparring events with filters
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<SparringEventDTO>>> GetSparringEvents(
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
                    WHERE s.Sport IN ('Arnis', 'Boxing', 'Taekwondo', 'Wrestling', 'Wushu')
                    AND e.Deleted = 0";

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

                sql += " ORDER BY s.Sport, ssc.Subcategory, sgc.Gender, sl.Level, e.Date, e.Time";

                var events = await _db.QueryAsync<SparringEventDTO, dynamic>(sql, parameters);

                if (events == null || !events.Any())
                    return NotFound("No sparring events found with the selected filters");

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sparring events");
                return StatusCode(500, $"Error retrieving sparring events: {ex.Message}");
            }
        }

        // GET match sets summary for an event
        [HttpGet("MatchSets/{eventId}")]
        public async Task<ActionResult> GetMatchSets(string eventId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        ss.MatNo,
                        ss.RoundNo,
                        ss.Corner,
                        ss.Result
                    FROM SparringScoring ss
                    WHERE ss.EventID = @EventID 
                    ORDER BY ss.MatNo, ss.RoundNo, ss.Corner;
                ";

                var records = await _db.QueryAsync<dynamic, dynamic>(sql, new { EventID = eventId });

                var matches = new List<object>();

                var groupedByMat = records.GroupBy(r => (int)r.MatNo);
                foreach (var matGroup in groupedByMat)
                {
                    var matName = $"Mat {matGroup.Key}";
                    var sets = new List<object>();

                    var groupedByRound = matGroup.GroupBy(r => (int)r.RoundNo);
                    foreach (var roundGroup in groupedByRound)
                    {
                        var roundNumber = roundGroup.Key;
                        var redRecord = roundGroup.FirstOrDefault(r => r.Corner == "Red");
                        var blueRecord = roundGroup.FirstOrDefault(r => r.Corner == "Blue");

                        string setWinner = "Not Played";
                        bool isCompleted = false;

                        if (redRecord != null && blueRecord != null)
                        {
                            if (redRecord.Result != null && blueRecord.Result != null)
                            {
                                isCompleted = true;

                                if (redRecord.Result == "WIN" && blueRecord.Result == "LOSS")
                                    setWinner = "Red";
                                else if (blueRecord.Result == "WIN" && redRecord.Result == "LOSS")
                                    setWinner = "Blue";
                                else if (redRecord.Result == "DRAW" && blueRecord.Result == "DRAW")
                                    setWinner = "Draw";
                            }
                        }

                        int redScore = 0;
                        int blueScore = 0;

                        if (isCompleted)
                        {
                            if (setWinner == "Red")
                            {
                                redScore = 1;
                                blueScore = 0;
                            }
                            else if (setWinner == "Blue")
                            {
                                redScore = 0;
                                blueScore = 1;
                            }
                            else if (setWinner == "Draw")
                            {
                                redScore = 0;
                                blueScore = 0;
                            }
                        }

                        sets.Add(new
                        {
                            SetNumber = roundNumber,
                            RedScore = redScore,
                            BlueScore = blueScore,
                            Winner = setWinner,
                            IsCompleted = isCompleted,
                            RedResult = redRecord?.Result,
                            BlueResult = blueRecord?.Result
                        });
                    }

                    var completedSets = sets.Where(s => ((dynamic)s).IsCompleted).ToList();
                    var redWins = completedSets.Count(s => ((dynamic)s).Winner == "Red");
                    var blueWins = completedSets.Count(s => ((dynamic)s).Winner == "Blue");

                    string matchWinner = "Not Completed";
                    bool matchCompleted = false;

                    if (redWins >= 2)
                    {
                        matchWinner = "Red";
                        matchCompleted = true;
                    }
                    else if (blueWins >= 2)
                    {
                        matchWinner = "Blue";
                        matchCompleted = true;
                    }
                    else if (completedSets.Count >= 3)
                    {
                        var totalRedPoints = completedSets.Sum(s => ((dynamic)s).RedScore);
                        var totalBluePoints = completedSets.Sum(s => ((dynamic)s).BlueScore);

                        if (totalRedPoints > totalBluePoints)
                            matchWinner = "Red";
                        else if (totalBluePoints > totalRedPoints)
                            matchWinner = "Blue";
                        else
                            matchWinner = "Draw";

                        matchCompleted = true;
                    }

                    matches.Add(new
                    {
                        MatName = matName,
                        MatNo = matGroup.Key,
                        Sets = sets,
                        MatchWinner = matchWinner,
                        MatchCompleted = matchCompleted
                    });
                }

                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching match sets for EventID {EventID}", eventId);
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // POST create/update sparring scoring record
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateSparringDTO data)
        {
            try
            {
                if (data == null)
                    return BadRequest("Invalid sparring scoring data.");

                string checkSql = @"
                    SELECT COUNT(*) FROM SparringScoring 
                    WHERE EventID = @EventID 
                    AND RegionID = @RegionID 
                    AND RoundNo = @RoundNo 
                    AND MatNo = @MatNo 
                    AND Corner = @Corner;
                ";

                int existingCount = await _db.ExecuteScalarAsync<int, dynamic>(checkSql, new
                {
                    data.EventID,
                    data.RegionID,
                    data.RoundNo,
                    data.MatNo,
                    data.Corner
                });

                if (existingCount > 0)
                {
                    string updateSql = @"
                        UPDATE SparringScoring
                        SET 
                            SportsID = @SportsID,
                            EventVersusID = @EventVersusID,
                            Result = @Result,
                            PlayerID = @PlayerID,
                            UpdatedAt = GETDATE()
                        WHERE EventID = @EventID 
                        AND RegionID = @RegionID 
                        AND RoundNo = @RoundNo 
                        AND MatNo = @MatNo 
                        AND Corner = @Corner;
                    ";

                    await _db.ExecuteAsync<dynamic>(updateSql, new
                    {
                        data.SportsID,
                        data.EventVersusID,
                        data.Result,
                        data.PlayerID,
                        data.EventID,
                        data.RegionID,
                        data.RoundNo,
                        data.MatNo,
                        data.Corner
                    });

                    return Ok(0);
                }
                else
                {
                    string insertSql = @"
                        INSERT INTO SparringScoring (
                            SportsID, EventID, EventVersusID, RegionID, RoundNo, 
                            MatNo, Corner, Result, PlayerID, CreatedAt, UpdatedAt
                        ) VALUES (
                            @SportsID, @EventID, @EventVersusID, @RegionID, @RoundNo, 
                            @MatNo, @Corner, @Result, @PlayerID, GETDATE(), GETDATE()
                        );
                        SELECT CAST(SCOPE_IDENTITY() as int);
                    ";

                    int newId = await _db.ExecuteScalarAsync<int, dynamic>(insertSql, new
                    {
                        data.SportsID,
                        data.EventID,
                        data.EventVersusID,
                        data.RegionID,
                        data.RoundNo,
                        data.MatNo,
                        data.Corner,
                        data.Result,
                        data.PlayerID
                    });

                    if (newId > 0)
                        return Ok(newId);

                    return BadRequest("Failed to insert SparringScoring record.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SparringScoring record");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT update sparring scoring record
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSparringDTO data)
        {
            try
            {
                if (data == null)
                    return BadRequest("Invalid update data.");

                string sql = @"
                    UPDATE SparringScoring
                    SET 
                        MatNo = ISNULL(@MatNo, MatNo),
                        Corner = ISNULL(@Corner, Corner),
                        Result = ISNULL(@Result, Result),
                        RoundNo = ISNULL(@RoundNo, RoundNo),
                        PlayerID = ISNULL(@PlayerID, PlayerID),
                        UpdatedAt = GETDATE()
                    WHERE ID = @ID;
                ";

                await _db.ExecuteAsync<dynamic>(sql, new
                {
                    ID = id,
                    data.MatNo,
                    data.Corner,
                    data.Result,
                    data.RoundNo,
                    data.PlayerID
                });

                return Ok(new { message = "SparringScoring updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SparringScoring with ID {ID}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE assignment by region and round
        [HttpDelete("region/{regionId}/round/{roundNo}")]
        public async Task<IActionResult> DeleteAssignment(int regionId, int roundNo)
        {
            try
            {
                string sql = "DELETE FROM SparringScoring WHERE RegionID = @RegionID AND RoundNo = @RoundNo";
                await _db.ExecuteAsync<dynamic>(sql, new { RegionID = regionId, RoundNo = roundNo });

                return Ok(new { message = "Sparring scoring records deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sparring scoring records for RegionID {RegionID} and RoundNo {RoundNo}", regionId, roundNo);
                return StatusCode(500, $"Error deleting sparring scoring records: {ex.Message}");
            }
        }
    }
}