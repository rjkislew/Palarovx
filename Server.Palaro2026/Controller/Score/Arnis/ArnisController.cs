using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;

namespace Server.Palaro2026.Controller.Score.Arnis;

[Route("api/[controller]")]
[ApiController]
public class ArnisController : ControllerBase
{
    private readonly ISqlDataAccess _db;
    private readonly ILogger<ArnisController> _logger;
    
    public ArnisController(ISqlDataAccess db, ILogger<ArnisController> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    [HttpGet("events")]
    public async Task<ActionResult<IEnumerable<EventDTO.EventDetails>>> GetEvents(
        [FromQuery] string? sport = null,
        [FromQuery] string? subcategory = null,
        [FromQuery] string? gender = null,
        [FromQuery] string? level = null,
        [FromQuery] string? eventStage = null,
        [FromQuery] string? eventID = null,
        [FromQuery] string? mainCategory = null)
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
                 sr.ID as RegionID
             FROM Events e
             INNER JOIN SportSubcategories ssc ON e.SportSubcategoryID = ssc.ID
             INNER JOIN Sports s ON ssc.SportID = s.ID
             INNER JOIN SportGenderCategories sgc ON ssc.SportGenderCategoryID = sgc.ID
             INNER JOIN SchoolLevels sl ON ssc.SchoolLevelID = sl.ID
             LEFT JOIN EventStages es ON e.EventStageID = es.ID
             LEFT JOIN EventVersusTeams evt ON e.ID = evt.EventID
             LEFT JOIN SchoolRegions sr ON evt.SchoolRegionID = sr.ID
             WHERE 1=1";

            var conditions = new List<string>();
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(eventID))
            {
                conditions.Add("e.ID = @EventID");
                parameters.Add("EventID", eventID);
            }
            
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
            
            if (!string.IsNullOrEmpty(mainCategory))  // Add this condition
            {
                conditions.Add("e.SportMainCat = @MainCategory");
                parameters.Add("MainCategory", mainCategory);
            }

            if (conditions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditions);
            }

            sql += " ORDER BY e.Date, e.Time";

            var events = await _db.QueryAsync<EventDTO.EventDetails, dynamic>(sql, parameters);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching chess events");
            return StatusCode(500, $"Error retrieving chess events: {ex.Message}");
        }
    }
}