using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.Services.UploadServices;

namespace Server.Palaro2026.Controller;

[ApiController]
[Route("api/[controller]")]
public class CoachImportController : ControllerBase
{
    private readonly ICoachImportService _importService;
    private readonly ILogger<CoachImportController> _logger;

    public CoachImportController(ICoachImportService importService, ILogger<CoachImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [HttpPost("UploadCoaches")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadCoaches([FromForm] IFormFile? file)
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

            var result = await _importService.ImportCoachesFromExcelAsync(ms, User?.Identity?.Name ?? "web");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed.");
            return StatusCode(500, new { Message = "Import failed.", Error = ex.Message });
        }
    }
}