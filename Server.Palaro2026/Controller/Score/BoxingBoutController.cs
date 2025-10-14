
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using Server.Palaro2026.Services;
using System.Data;


namespace Server.Palaro2026.Controller.Score
{
    [Route("api/score/[controller]")]
    [ApiController]
    public class BoxingBoutController : ControllerBase
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<BoxingBoutController> _logger;

        public BoxingBoutController(ISqlDataAccess db, ILogger<BoxingBoutController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/BoxingBout/regions
        [HttpGet("regions")]
        public async Task<ActionResult<IEnumerable<RegionBoutDto>>> GetRegions()
        {
            string sql = " SELECT ID,Region as 'Name',Abbreviation,Color FROM SchoolRegions";
            var regions = await _db.QueryAsync<RegionBoutDto>(sql);
            return Ok(regions);
        }

        // GET: api/BoxingBout/arenas
        [HttpGet("arenas")]
        public async Task<ActionResult<IEnumerable<ArenaDTO>>> GetArenas()
        {
            string sql = " SELECT ID,name as 'Name' FROM ref_arenas where sport_id = 8";
            var regions = await _db.QueryAsync<ArenaDTO>(sql);
            return Ok(regions);
        }

        // GET: api/BoxingBout/referees
        [HttpGet("referees")]
        public async Task<ActionResult<IEnumerable<RefereeDTO>>> GetReferees()
        {
            string sql = @$"SELECT  id, LTRIM(RTRIM( first_name +  CASE   WHEN middle_name IS NOT NULL AND middle_name <> ''
                                THEN ' ' + LEFT(middle_name, 1) + '.' ELSE '' END +  ' ' + last_name )) AS 'Name'
                                FROM Palaro2026.dbo.ref_referees where sports_id = 8;";
            var regions = await _db.QueryAsync<RefereeDTO>(sql);
            return Ok(regions);
        }

        // GET: api/BoxingBout/majorcategories
        [HttpGet("majorcategories")]
        public async Task<ActionResult<IEnumerable<MajorCategoryDTO>>> GetMajorCategories()
        {
            string sql = @$"SELECT id ,Majorcategory as 'Name' FROM SportMajorCategory where SportID = 8;";
            var regions = await _db.QueryAsync<MajorCategoryDTO>(sql);
            return Ok(regions);
        }

        // GET: api/BoxingBout/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            string sql = @$"SELECT  id ,Subcategory as 'Name' ,MajorCategoryID as 'LevelId'  FROM SportSubcategories where SportID = 8;";
            var regions = await _db.QueryAsync<CategoryDTO>(sql);
            return Ok(regions);
        }

        // GET: api/BoxingBout/rounds
        [HttpGet("rounds")]
        public async Task<ActionResult<IEnumerable<RoundsDTO>>> GetRounds()
        {
            string sql = @$"SELECT  id ,name as 'Name',abbreviation as 'Abbreviation' FROM ref_rounds;";
            var regions = await _db.QueryAsync<RoundsDTO>(sql);
            return Ok(regions);
        }

        // GET: api/BoxingBout/bouts
        [HttpGet("bouts")]
        public async Task<ActionResult<IEnumerable<BoutDTO>>> GetBouts()
        {
            string sql = @$"SELECT  id ,name as 'Name' FROM ref_bouts;";
            var regions = await _db.QueryAsync<BoutDTO>(sql);
            return Ok(regions);
        }

        // GET: api/BoxingBout/participants
        [HttpGet("participants/{CategoryId}")]
        public async Task<ActionResult<IEnumerable<ParticipantsDto>>> GetParticipants(int CategoryId)
        {
            string sql = @$"SELECT ID,ParticipantName,SportSubcategoryID,CoachName,SchoolRegionID,Region,Abbreviation FROM Palaro2026.dbo.vw_BoxingParticipants WHERE SportSubcategoryID =  {CategoryId}";
            var regions = await _db.QueryAsync<ParticipantsDto>(sql);
            return Ok(regions);
        }


        // ✅ 1. Get all boxing matches (Event_DrawOfMatch)
        [HttpGet("list_match")]
        public async Task<IActionResult> GetListMatch()
        {
            string sql = @"
                SELECT 
                    a.id,
                    a.sport_id,
                    a.majorcategories_id,
                    b.Majorcategory,
                    a.categories_id,
                    c.Subcategory,
                    a.bout_id,
                    a.round_id,
                    f.name AS round_name,
                    a.arena_id,
                    g.name AS arena_name,
                    a.schedule,
                    a.user_id,
                    a.datentime
                FROM Palaro2026.dbo.Event_DrawOfMatch AS a
                LEFT JOIN Palaro2026.dbo.SportMajorCategory AS b 
                    ON a.majorcategories_id = b.id AND b.SportID = 8
                LEFT JOIN Palaro2026.dbo.SportSubcategories AS c 
                    ON a.categories_id = c.ID AND c.SportID = 8
                LEFT JOIN Palaro2026.dbo.ref_rounds AS f 
                    ON a.round_id = f.id
                LEFT JOIN Palaro2026.dbo.ref_arenas AS g 
                    ON a.arena_id = g.id AND g.sport_id = 8
                ORDER BY a.id;
            ";

            var result = await _db.QueryAsync<EventDrawOfMatchDTO, dynamic>(sql, new { });
            return Ok(result);
        }

        // ✅ 2. Get participants for a specific match (Event_DrawOfMatchParticipants)
        [HttpGet("list_participants/{matchId}")]
        public async Task<IActionResult> GetListParticipants(int matchId)
        {
            string sql = @"
                SELECT 
                    e.id,
                    match_id,
                    side_id,
                    e.name AS side_name,
                    region_id,
                    participant_id,
                    participant_name,
                    region_name,
                    region_abbr,
                    coach_name
                FROM Palaro2026.dbo.Event_DrawOfMatchParticipants AS d
                LEFT JOIN Palaro2026.dbo.ref_sides AS e
                    ON d.side_id = e.id AND sport_id = 8
                WHERE match_id = @matchId;
            ";

            var result = await _db.QueryAsync<EventDrawOfMatchParticipantsDTO, dynamic>(sql, new { matchId });
            return Ok(result);
        }














        //INSERT SERCTION

        [HttpPost("SaveMatch")]
        public async Task<ActionResult<int>> SaveMatch([FromBody] CreateBoxingMatchDto data)
        {
            try
            {
                if (data == null)
                {
                    return BadRequest("Match data is required");
                }

                // Validate required fields
                if (data.BoutId == 0)
                    return BadRequest("Bout ID is required");

                if (data.MajorCategoryId == 0)
                    return BadRequest("Major category is required");

                if (data.CategoryId == 0)
                    return BadRequest("Category is required");

                if (string.IsNullOrEmpty(data.RedCorner?.ParticipantId) || string.IsNullOrEmpty(data.BlueCorner?.ParticipantId))
                    return BadRequest("Both fighters are required");

                // Parse arena ID from string (frontend sends arena ID as string)
                if (!int.TryParse(data.Arena, out int arenaId) || arenaId == 0)
                {
                    // If parsing fails, try to get arena ID by name as fallback
                    arenaId = await GetArenaIdByName(data.Arena);
                    if (arenaId == 0)
                    {
                        return BadRequest("Invalid arena");
                    }
                }

                // First insert into Event_DrawOfMatch with EXACT column names
                var matchSql = @"
            INSERT INTO Event_DrawOfMatch 
            (sport_id, majorcategories_id, categories_id, bout_id, round_id, arena_id, schedule, user_id, datentime)
            VALUES 
            (8, @majorcategories_id, @categories_id, @bout_id, @round_id, @arena_id, @schedule, @user_id, GETDATE());
            
            SELECT CAST(SCOPE_IDENTITY() as int);";


                var matchId = await _db.ExecuteScalarAsync<int, dynamic>(matchSql,
                    new
                    {
                        majorcategories_id = data.MajorCategoryId,
                        categories_id = data.CategoryId,
                        bout_id = data.BoutId,
                        round_id = data.RoundId,
                        arena_id = arenaId,
                        schedule = data.Schedule,
                        user_id = data.UserId
                    },
                    System.Data.CommandType.Text);

                if (matchId > 0)
                {
                    // Now insert participants into Event_DrawOfMatchParticipants with EXACT column names
                    var participantSql = @"
                INSERT INTO Event_DrawOfMatchParticipants 
                (match_id, side_id, region_id, participant_id, participant_name, region_name, region_abbr, coach_name)
                VALUES 
                (@match_id, @side_id, @region_id, @participant_id, @participant_name, @region_name, @region_abbr, @coach_name);";

                    // Insert Red Corner (side_id = 1)
                    await _db.ExecuteAsync<dynamic>(participantSql,
                        new
                        {
                            match_id = matchId,
                            side_id = 1, // Red
                            region_id = data.RedCorner.RegionId,
                            participant_id = data.RedCorner.ParticipantId,
                            participant_name = data.RedCorner.ParticipantName,
                            region_name = data.RedCorner.Region,
                            region_abbr = data.RedCorner.Abbreviation,
                            coach_name = data.RedCorner.CoachName
                        },
                        System.Data.CommandType.Text);

                    // Insert Blue Corner (side_id = 2)
                    await _db.ExecuteAsync<dynamic>(participantSql,
                        new
                        {
                            match_id = matchId,
                            side_id = 2, // Blue
                            region_id = data.BlueCorner.RegionId,
                            participant_id = data.BlueCorner.ParticipantId,
                            participant_name = data.BlueCorner.ParticipantName,
                            region_name = data.BlueCorner.Region,
                            region_abbr = data.BlueCorner.Abbreviation,
                            coach_name = data.BlueCorner.CoachName
                        },
                        System.Data.CommandType.Text);

                    return Ok(matchId);
                }

                return BadRequest("Failed to create match");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving match: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to get arena ID by name
        private async Task<int> GetArenaIdByName(string arenaName)
        {
            try
            {
                if (string.IsNullOrEmpty(arenaName))
                    return 1; // Default arena ID

                var sql = "SELECT id FROM Arenas WHERE name = @ArenaName";
                var result = await _db.QueryFirstAsync<int, dynamic>(sql, new { ArenaName = arenaName }, System.Data.CommandType.Text);
                return result;
            }
            catch
            {
                return 1; // Default arena ID if not found
            }
        }

       














        //INSERT SERCTION



    }
}
