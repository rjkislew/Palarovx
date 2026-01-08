using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.Services.UploadServices;
using System.Security.Claims;

namespace Server.Palaro2026.Controller;

[ApiController]
[Route("api/[controller]")]
public class PlayerImportController : ControllerBase
{
    private readonly IPlayerImportService _importService;
    private readonly ILogger<PlayerImportController> _logger;

    public PlayerImportController(IPlayerImportService importService, ILogger<PlayerImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [HttpPost("UploadPlayers")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadPlayers([FromForm] IFormFile? file, [FromForm] string? uploadedByUserId = null)
    {

        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "No file uploaded." });

        var ext = Path.GetExtension(file.FileName);
        if (!string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { Message = "Only .xlsx files are supported." });

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            string userId;
            var loggedInUserId = GetLoggedInUserId();

            if (!string.IsNullOrEmpty(loggedInUserId))
            {
                userId = loggedInUserId;
            }
            else if (!string.IsNullOrEmpty(uploadedByUserId))
            {
                userId = uploadedByUserId;
            }
            else
            {
                userId = "unknown"; // Fallback
            }

            var result = await _importService.ImportPlayersFromExcelAsync(ms, userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed.");
            return StatusCode(500, new { Message = "Import failed.", Error = ex.Message });
        }
    }

    private string? GetLoggedInUserId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
               User?.FindFirstValue("sub");
    }
}