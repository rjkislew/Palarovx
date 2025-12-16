using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;

namespace Server.Palaro2026.Controller.Score.PerformanceBased;

[Route("api/[controller]")]
[ApiController]
public class PerformanceScoreController : ControllerBase
{
    private readonly ISqlDataAccess _db;
    private readonly ILogger<PerformanceScoreController> _logger;

    public PerformanceScoreController(ISqlDataAccess db, ILogger<PerformanceScoreController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("Scores")]
    public async Task<IActionResult> GetAllScores()
    {
        try
        {
            string sql = @"
                SELECT ID, PerformanceID, PerformanceTeamID, SportSubcategoryID, Score, Rank, UserID, UpdatedAt, TeamID
                FROM [Palaro2026].[dbo].[PerformanceScore]
                ORDER BY ID;";

            var scores = await _db.QueryAsync<PerformanceBasedDTO.PerformanceScore, dynamic>(sql, new { });
            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all scores");
            return StatusCode(500, "Error retrieving scores");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetScoreById(int id)
    {
        try
        {
            string sql = @"
                SELECT ID, PerformanceID, PerformanceTeamID, SportSubcategoryID, Score, Rank, UserID, UpdatedAt, TeamID
                FROM [Palaro2026].[dbo].[PerformanceScore]
                WHERE ID = @ID;";

            var result = await _db.QueryAsync<PerformanceBasedDTO.PerformanceScore, dynamic>(sql, new { ID = id });

            var record = result.FirstOrDefault();
            if (record == null)
                return NotFound($"Score with ID {id} not found.");

            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting record with ID {id}");
            return StatusCode(500, "Error retrieving record");
        }
    }

    [HttpGet("Score/{performanceId}")]
    public async Task<ActionResult<List<PerformanceBasedDTO.PerformanceScore>>> GetScoreByEvent(int performanceId)
    {
        try
        {
            string sql = @"
            SELECT ID, PerformanceID, PerformanceTeamID, SportSubcategoryID, Score, Rank, UserID, UpdatedAt, TeamID
                FROM [Palaro2026].[dbo].[PerformanceScore]
            WHERE PerformanceID = @performanceId;";

            var score = await _db.QueryAsync<PerformanceBasedDTO.PerformanceScore, dynamic>(sql, new { performanceId });

            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching scores for event {PerformanceID}", performanceId);
            return StatusCode(500, "Error retrieving scores");
        }
    }

    [HttpPost("Score")]
    public async Task<ActionResult<PerformanceBasedDTO.PerformanceScore>> CreateScore(
        [FromBody] PerformanceBasedDTO.PerformanceScore score)
    {
        try
        {
            string sql = @"
            INSERT INTO [Palaro2026].[dbo].[PerformanceScore]
            (PerformanceID, PerformanceTeamID, SportSubcategoryID, Score, Rank, UserID, UpdatedAt, TeamID)
            OUTPUT INSERTED.*
            VALUES (@PerformanceID, @PerformanceTeamID, @SportSubcategoryID, @Score, @Rank, @UserID, GETDATE(), @TeamID);";

            var parameters = new
            {
                PerformanceID = score.PerformanceID,
                PerformanceTeamID = score.PerformanceTeamID,
                SportSubcategoryID = score.SportSubcategoryID,
                Score = score.Score,
                Rank = score.Rank,
                UserID = score.UserID,
                TeamID = score.TeamID
            };

            var createdScore = (await _db.QueryAsync<PerformanceBasedDTO.PerformanceScore, dynamic>(sql, parameters))
                .FirstOrDefault();
            return Ok(createdScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating score");
            return StatusCode(500, $"Error creating score: {ex.Message}");
        }
    }
    
    [HttpPut("Score/{id}")]
    public async Task<ActionResult<PerformanceBasedDTO.PerformanceScore>> UpdateScore(int id, [FromBody] PerformanceBasedDTO.PerformanceScore request)
    {
        try
        {
            string sql = @"
            UPDATE [Palaro2026].[dbo].[PerformanceScore]
            SET 
                PerformanceID = @PerformanceID,
                PerformanceTeamID = @PerformanceTeamID,
                SportSubcategoryID = @SportSubcategoryID,
                Score = @Score,
                Rank = @Rank,
                UserID = @UserID,
                UpdatedAt = GETDATE()
            WHERE ID = @ID;

            SELECT * FROM [Palaro2026].[dbo].[PerformanceScore] WHERE ID = @ID;";

            request.ID = id;

            var updatedScore = (await _db.QueryAsync<PerformanceBasedDTO.PerformanceScore, dynamic>(sql, request))
                .FirstOrDefault();

            return Ok(updatedScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating score ID {id}");
            return StatusCode(500, "Error updating score");
        }
    }
    
    [HttpDelete("Score/{id}")]
    public async Task<IActionResult> DeleteScore(int id)
    {
        try
        {
            string sql = @"
                DELETE FROM [Palaro2026].[dbo].[PerformanceScore]
                WHERE ID = @ID;";

            await _db.ExecuteAsync(sql, new { ID = id });

            return Ok($"Score with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting score ID {id}");
            return StatusCode(500, "Error deleting score");
        }
    }
}