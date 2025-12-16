using System.Data;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Services;

namespace Server.Palaro2026.Controller;

[Route("api/[controller]")]
[ApiController]
public class GeneralScheduleController : ControllerBase
{
    private readonly ISqlDataAccess _db;
    private readonly ILogger<GeneralScheduleController> _logger;

    public GeneralScheduleController(ISqlDataAccess db, ILogger<GeneralScheduleController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ✅ GET: api/Schedule
    [HttpGet]
    public async Task<IActionResult> GetAllSchedules()
    {
        // Get main schedules
        string scheduleSql = @"SELECT ID, Date, Description, SportsID FROM Schedule ORDER BY Date;";
        var schedules = await _db.QueryAsync<GeneralScheduleDTO, dynamic>(scheduleSql, new { });

        // Get all activities at once
        string activitySql = @"SELECT ID, ScheduleID, ActivityName, Time FROM ScheduleActivities;";
        var allActivities = await _db.QueryAsync<ActivityDTO, dynamic>(activitySql, new { });

        // Group activities by ScheduleID
        var activitiesBySchedule = allActivities.GroupBy(a => a.ScheduleID)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Assign activities to each schedule
        foreach (var schedule in schedules)
        {
            if (activitiesBySchedule.TryGetValue(schedule.ID, out var activities))
            {
                schedule.Activities = activities;
            }
            else
            {
                schedule.Activities = new List<ActivityDTO>();
            }
        }

        return Ok(schedules);
    }

    // ✅ GET: api/Schedule/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<GeneralScheduleDTO>> GetScheduleById(int id)
    {
        try
        {
            // Get schedule
            string sql = "SELECT ID, Date, Description, SportsID FROM Schedule WHERE ID = @id;";
            var schedule = (await _db.QueryAsync<GeneralScheduleDTO, dynamic>(sql, new { id })).FirstOrDefault();
    
            if (schedule == null)
                return NotFound("Schedule not found.");

            // Get activities - FIXED: using correct SQL variable and converting to List
            string activitiesSql = "SELECT ID, ScheduleID, ActivityName, Time FROM ScheduleActivities WHERE ScheduleID = @id;";
            var activities = (await _db.QueryAsync<ActivityDTO, dynamic>(activitiesSql, new { id })).ToList();
    
            schedule.Activities = activities;
            return Ok(schedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching schedule {ID}", id);
            return StatusCode(500, "Error retrieving schedule");
        }
    }
    
    [HttpGet("sport/{sportId}")]
    public async Task<IActionResult> GetSchedulesBySport(int sportId)
    {
        try
        {
            // Get schedules for specific sport
            string scheduleSql = @"SELECT ID, Date, Description, SportsID FROM Schedule 
                              WHERE SportsID = @sportId 
                              ORDER BY Date;";
            var schedules = await _db.QueryAsync<GeneralScheduleDTO, dynamic>(scheduleSql, new { sportId });

            // Get all activities for these schedules
            if (schedules.Any())
            {
                var scheduleIds = schedules.Select(s => s.ID).ToList();
            
                // Using Dapper's parameterized IN clause
                string activitySql = @"SELECT ID, ScheduleID, ActivityName, Time 
                                  FROM ScheduleActivities 
                                  WHERE ScheduleID IN @scheduleIds;";
            
                var allActivities = await _db.QueryAsync<ActivityDTO, dynamic>(activitySql, new { scheduleIds });

                // Group activities by ScheduleID
                var activitiesBySchedule = allActivities.GroupBy(a => a.ScheduleID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Assign activities to each schedule
                foreach (var schedule in schedules)
                {
                    if (activitiesBySchedule.TryGetValue(schedule.ID, out var activities))
                    {
                        schedule.Activities = activities;
                    }
                    else
                    {
                        schedule.Activities = new List<ActivityDTO>();
                    }
                }
            }

            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching schedules for sport {SportId}", sportId);
            return StatusCode(500, "Error retrieving schedules");
        }
    }

    // ✅ POST: api/Schedule
    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] GeneralScheduleDTO schedule)
    {
        if (schedule == null)
            return BadRequest("Schedule data is required.");

        try
        {
            string scheduleSql = @"
        INSERT INTO Schedule (Date, Description, SportsID)
        VALUES (@Date, @Description, @SportsID);
        SELECT CAST(SCOPE_IDENTITY() as int);
    ";

            var scheduleId = await _db.ExecuteScalarAsync<int, dynamic>(scheduleSql, new
            {
                Date = schedule.Date ?? DateTime.Now,
                schedule.Description,
                SportsID = schedule.SportsID ?? 0
            });

            if (schedule.Activities != null && schedule.Activities.Any())
            {
                string activitySql = @"
            INSERT INTO ScheduleActivities (ScheduleID, ActivityName, Time)
            VALUES (@ScheduleID, @ActivityName, @Time);
        ";

                foreach (var activity in schedule.Activities)
                {
                    if (!string.IsNullOrEmpty(activity.ActivityName))
                    {
                        await _db.ExecuteAsync<dynamic>(activitySql, new 
                        { 
                            ScheduleID = scheduleId,
                            ActivityName = activity.ActivityName,
                            Time = activity.Time  // ADDED: Store time per activity
                        });
                    }
                }
            }

            _logger.LogInformation("Added new schedule on {Date}", schedule.Date);
            return Ok(new { message = "Schedule created successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            return StatusCode(500, "Error creating schedule");
        }
    }

    // ✅ PUT: api/Schedule/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] GeneralScheduleDTO schedule)
    {
        if (schedule == null || id != schedule.ID)
            return BadRequest("Invalid data.");

        try
        {
            // Update schedule
            string updateSql = @"
            UPDATE Schedule 
            SET Date = @Date, Description = @Description, SportsID = @SportsID
            WHERE ID = @ID;
        ";

            await _db.ExecuteAsync(updateSql, new
            {
                ID = id,
                Date = schedule.Date ?? DateTime.Now,
                schedule.Description,
                SportsID = schedule.SportsID ?? 0
            });

            // Update activities
            string deleteActivities = "DELETE FROM ScheduleActivities WHERE ScheduleID = @ID;";
            await _db.ExecuteAsync(deleteActivities, new { ID = id });

            if (schedule.Activities?.Count > 0)
            {
                string insertActivity = @"
                INSERT INTO ScheduleActivities (ScheduleID, ActivityName, Time)
                VALUES (@ScheduleID, @ActivityName, @Time);
            ";

                foreach (var activity in schedule.Activities)
                {
                    await _db.ExecuteAsync(insertActivity, new 
                    { 
                        ScheduleID = id,
                        activity.ActivityName,
                        activity.Time
                    });
                }
            }

            return Ok(new { message = "Schedule updated." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule {ID}", id);
            return StatusCode(500, "Update failed");
        }
    }

    // ✅ DELETE: api/Schedule/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        try
        {
            // Delete activities first
            await _db.ExecuteAsync(
                "DELETE FROM ScheduleActivities WHERE ScheduleID = @ID;", 
                new { ID = id }
            );

            // Delete schedule
            await _db.ExecuteAsync(
                "DELETE FROM Schedule WHERE ID = @ID;", 
                new { ID = id }
            );

            return Ok(new { message = "Schedule deleted." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ID}", id);
            return StatusCode(500, "Delete failed");
        }
    }
}