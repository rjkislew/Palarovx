using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;

namespace Server.Palaro2026.Controller.Score.PerformanceBased;

[Route("api/[controller]")]
[ApiController]
public class PerformanceBasedController : ControllerBase
{
    private readonly ISqlDataAccess _db;
    private readonly ILogger<PerformanceBasedController> _logger;

    public PerformanceBasedController(ISqlDataAccess db, ILogger<PerformanceBasedController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("PerformanceEvents")]
    public async Task<IActionResult> GetFilteredEvents(
        int? sportID,
        int? schoolLevelID,
        int? genderID,
        string? mainCategory,
        int? stageID,
        bool? isFinished)
    {
        try
        {
            string sql = @"
            SELECT ID, SportID, MainCategory, LevelID, GenderID, StageID, IsFinished
            FROM [Palaro2026].[dbo].[PerformanceEvent]
            WHERE (@sportID IS NULL OR SportID = @sportID)
              AND (@schoolLevelID IS NULL OR LevelID = @schoolLevelID)
              AND (@genderID IS NULL OR GenderID = @genderID)
              AND (@mainCategory IS NULL OR MainCategory = @mainCategory)
              AND (@stageID IS NULL OR StageID = @stageID)
              AND (@isFinished IS NULL OR IsFinished = @isFinished)
            ORDER BY ID;";

            var events = await _db.QueryAsync<PerformanceBasedDTO.PerformanceEvent, dynamic>(sql,
                new { sportID, schoolLevelID, genderID, mainCategory, stageID, isFinished });

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering records");
            return StatusCode(500, "Error retrieving records");
        }
    }


    [HttpGet("PerformanceEvent/{id}")]
    public async Task<ActionResult<PerformanceBasedDTO.PerformanceEvent>> GetEventByID(int id)
    {
        try
        {
            string sql = @"
                SELECT ID, SportID, MainCategory, LevelID, GenderID, StageID, IsFinished 
                FROM [Palaro2026].[dbo].[PerformanceEvent] 
                WHERE ID = @id;";

            var events = (await _db.QueryAsync<PerformanceBasedDTO.PerformanceEvent, dynamic>(sql, new { id }))
                .FirstOrDefault();

            if (events == null)
                return NotFound("Event record not found.");

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event record {ID}", id);
            return StatusCode(500, "Error retrieving event record");
        }
    }

    [HttpGet("OpposingTeams")]
    public async Task<IActionResult> GetAllOpposingTeams()
    {
        try
        {
            string sql = @"
            SELECT ID, PerformanceID, RegionID, PlayerID, TeamID 
            FROM [Palaro2026].[dbo].[PerformanceTeam] 
            ORDER BY PerformanceID, ID;";

            var teams = await _db.QueryAsync<PerformanceBasedDTO.PerformanceTeam, dynamic>(sql, new { });
            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all opposing teams");
            return StatusCode(500, "Error retrieving opposing teams");
        }
    }

    [HttpGet("OpposingTeams/{eventId}")]
    public async Task<ActionResult<List<PerformanceBasedDTO.PerformanceTeam>>> GetOpposingTeamsByEvent(int eventId)
    {
        try
        {
            string sql = @"
            SELECT ID, PerformanceID, RegionID, PlayerID, TeamID 
            FROM [Palaro2026].[dbo].[PerformanceTeam] 
            WHERE PerformanceID = @eventId;";

            var teams = await _db.QueryAsync<PerformanceBasedDTO.PerformanceTeam, dynamic>(sql, new { eventId });

            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching opposing teams for event {EventId}", eventId);
            return StatusCode(500, "Error retrieving opposing teams");
        }
    }

    [HttpGet("OpposingTeam/{id}")]
    public async Task<ActionResult<List<PerformanceBasedDTO.PerformanceTeam>>> GetOpposingTeamsByID(int id)
    {
        try
        {
            string sql = @"
            SELECT ID, PerformanceID, RegionID, PlayerID, TeamID 
            FROM [Palaro2026].[dbo].[PerformanceTeam] 
            WHERE ID = @id;";

            var teams = await _db.QueryAsync<PerformanceBasedDTO.PerformanceTeam, dynamic>(sql, new { id });

            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching opposing teams for event {ID}", id);
            return StatusCode(500, "Error retrieving opposing teams");
        }
    }


    [HttpPost("AddEvent")]
    public async Task<ActionResult<PerformanceBasedDTO.PerformanceEvent>> CreateEvent(
        [FromBody] PerformanceBasedDTO.PerformanceEvent events)
    {
        try
        {
            string sql = @"
                INSERT INTO [Palaro2026].[dbo].[PerformanceEvent] 
                (SportID, MainCategory, LevelID, GenderID, StageID, IsFinished)
                OUTPUT INSERTED.*
                VALUES (@SportID, @MainCategory, @LevelID, @GenderID, @StageID, @IsFinished);";

            var parameters = new
            {
                events.SportID,
                events.MainCategory,
                events.LevelID,
                events.GenderID,
                events.StageID,
                events.IsFinished
            };

            var createdEvent = (await _db.QueryAsync<PerformanceBasedDTO.PerformanceEvent, dynamic>(sql, parameters))
                .FirstOrDefault();

            return CreatedAtAction(nameof(GetEventByID), new { id = createdEvent.ID }, createdEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event record");
            return StatusCode(500, "Error creating event record");
        }
    }

    [HttpPost("AddOpposingTeam")]
    public async Task<ActionResult<PerformanceBasedDTO.PerformanceTeam>> CreateOpposingTeam(
        [FromBody] PerformanceBasedDTO.PerformanceTeam opposingTeam)
    {
        try
        {
            string sql = @"
            INSERT INTO [Palaro2026].[dbo].[PerformanceTeam] 
            (PerformanceID, RegionID, PlayerID, TeamID)
            OUTPUT INSERTED.*
            VALUES (@PerformanceID, @RegionID, @PlayerID, @TeamID);";

            var parameters = new
            {
                opposingTeam.PerformanceID,
                opposingTeam.TeamID,
                opposingTeam.RegionID,
                opposingTeam.PlayerID
            };

            var createdTeam = (await _db.QueryAsync<PerformanceBasedDTO.PerformanceTeam, dynamic>(sql, parameters))
                .FirstOrDefault();

            return CreatedAtAction(nameof(GetOpposingTeamsByID), new { id = createdTeam.ID }, createdTeam);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opposing team record");
            return StatusCode(500, "Error creating opposing team record");
        }
    }

    [HttpPut("UpdateEvent/{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] PerformanceBasedDTO.PerformanceEvent performance)
    {
        try
        {
            string sql = @"
                UPDATE [Palaro2026].[dbo].[PerformanceEvent]
                SET 
                    SportID = @SportID,
                    MainCategory = @MainCategory,
                    LevelID = @LevelID,
                    GenderID = @GenderID,
                    StageID = @StageID,
                    IsFinished = @IsFinished
                WHERE ID = @ID;";

            var parameters = new
            {
                ID = id,
                performance.SportID,
                performance.MainCategory,
                performance.LevelID,
                performance.GenderID,
                performance.StageID,
                performance.IsFinished
            };

            await _db.ExecuteAsync(sql, parameters);
            return Ok($"Event with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event record {ID}", id);
            return StatusCode(500, "Error updating event record");
        }
    }


    [HttpDelete("DeleteEvent/{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        try
        {
            await _db.ExecuteAsync("DELETE FROM [Palaro2026].[dbo].[PerformanceTeam] WHERE PerformanceID = @id", new { id });
            await _db.ExecuteAsync("DELETE FROM [Palaro2026].[dbo].[PerformanceEvent] WHERE ID = @id", new { id });

            return Ok("Event deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {ID}", id);
            return StatusCode(500, "Error deleting event");
        }
    }

    [HttpPut("UpdateOpposingTeam/{id}")]
    public async Task<IActionResult> UpdateOpposingTeam(int id, [FromBody] PerformanceBasedDTO.PerformanceTeam team)
    {
        try
        {
            string sql = @"
            UPDATE [Palaro2026].[dbo].[PerformanceTeam]
            SET 
                PerformanceID = @PerformanceID,
                RegionID = @RegionID,
                PlayerID = @PlayerID
            WHERE ID = @ID;";

            var parameters = new
            {
                ID = id,
                team.PerformanceID,
                team.RegionID,
                team.PlayerID
            };

            await _db.ExecuteAsync(sql, parameters);
            return Ok($"Opposing team with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating opposing team record {ID}", id);
            return StatusCode(500, "Error updating opposing team record");
        }
    }

    [HttpDelete("DeleteOpposingTeamsByEvent/{eventId}")]
    public async Task<IActionResult> DeleteOpposingTeamsByEvent(int eventId)
    {
        try
        {
            string sql = "DELETE FROM [Palaro2026].[dbo].[PerformanceTeam] WHERE PerformanceID = @eventId;";
            await _db.ExecuteAsync(sql, new { eventId });
            return Ok($"All opposing teams for event {eventId} deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting opposing teams for event {EventId}", eventId);
            return StatusCode(500, "Error deleting opposing teams");
        }
    }

    [HttpDelete("DeleteTeam/{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        try
        {
            await _db.ExecuteAsync("DELETE FROM [Palaro2026].[dbo].[PerformanceTeam] WHERE ID = @id", new { id });

            return Ok("Event deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {ID}", id);
            return StatusCode(500, "Error deleting event");
        }
    }

    [HttpPut("UploadAttachment/{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPerformanceEventAttachment(
    int id,
    [FromForm] IFormFile? attachmentFile)
    {
        if (attachmentFile == null || attachmentFile.Length == 0)
            return BadRequest("No file uploaded or file is empty.");

        // 1) Validate extension
        var allowedExtensions = new[] { ".jpeg", ".jpg", ".png", ".pdf", ".doc", ".docx" };
        var fileExtension = Path.GetExtension(attachmentFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Allowed: .jpeg, .jpg, .png, .pdf, .doc, .docx");

        // 2) Validate size (5MB)
        if (attachmentFile.Length > 10 * 1024 * 1024)
            return BadRequest("File size exceeds the 10 MB limit.");

        // 3) Save path
        var basePath = @"D:\pgas_attachment\palaro2026\media\events\official event records";
        Directory.CreateDirectory(basePath);

        var safeId = id.ToString(); // int is already safe for file name
        var fileName = $"{safeId}{fileExtension}";
        var fullPath = Path.Combine(basePath, fileName);

        // 4) Delete old versions (same id, different ext)
        foreach (var file in Directory.GetFiles(basePath, $"{safeId}.*"))
            System.IO.File.Delete(file);

        // 5) Save
        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await attachmentFile.CopyToAsync(stream);

        return Ok(new { message = "Attachment uploaded successfully.", fileName, storagePath = fullPath });
    }
}