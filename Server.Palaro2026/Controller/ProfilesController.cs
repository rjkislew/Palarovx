using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class ProfilesController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public ProfilesController(Palaro2026Context context)
        {
            _context = context;
        }
        // view coaches details

        [HttpGet("Coach/Details")] // /api/Profiles/Coach/Details
        public async Task<ActionResult<List<ProfilesDTO.ProfileCoachesDetails>>> GetProfileCoachesDetails()
        {
            try
            {
                var profileCoaches = await _context.ProfileCoaches
                    .Include(pc => pc.SchoolRegion)
                    .Include(pc => pc.Sport)
                    .Include(pc => pc.SportGenderCategories)
                    .Include(pc => pc.SchoolDivision)
                    .Include(pc => pc.School)
                    .ThenInclude(sl => sl!.SchoolLevels)
                    .AsNoTracking()
                    .ToListAsync();

                var profileCoachesDTO = profileCoaches.Select(profileCoach => new ProfilesDTO.ProfileCoachesDetails
                {
                    ID = profileCoach.ID,
                    FirstName = profileCoach.FirstName,
                    LastName = profileCoach.LastName,
                    MiddleInitial = profileCoach.MiddleInitial,
                    Sex = profileCoach.Sex,
                    BirthDate = profileCoach.BirthDate,
                    Designation = profileCoach.Designation,
                    Sports = profileCoach.Sport?.Sport,
                    SportGenderCategories = profileCoach.SportGenderCategories?.Gender,
                    Region = profileCoach.SchoolRegion?.Region,
                    Abbreviation = profileCoach.SchoolRegion?.Abbreviation,
                    SchoolDivision = profileCoach.SchoolDivision?.Division,
                    School = profileCoach.School?.School,
                    Level = profileCoach.School?.SchoolLevels?.Level,
                }).ToList();

                return Ok(profileCoachesDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Player Events view
        [HttpGet("Player/Events")] // /api/Profiles/Player/Events
        public async Task<ActionResult<List<ProfilesDTO.ProfilePlayerEvent>>> GetProfilePlayerEvent(
            [FromQuery] int? regionID,
            [FromQuery] int? subCategoryID)
        {
            try
            {
                var query = _context.ProfilePlayers
                    .Include(p => p.School)
                    .ThenInclude(sd => sd!.SchoolDivision)
                    .ThenInclude(sr => sr!.SchoolRegion)
                    .Include(p => p.ProfilePlayerSports)
                    .ThenInclude(ppsc => ppsc.SportSubcategory)
                    .AsQueryable();

                // Apply filters if provided
                if (regionID.HasValue)
                {
                    query = query.Where(p => p.School!.SchoolDivision!.SchoolRegion!.ID == regionID.Value);
                }

                if (subCategoryID.HasValue)
                {
                    query = query.Where(p => p.ProfilePlayerSports
                        .Any(ppsc => ppsc.SportSubcategory!.ID == subCategoryID.Value));
                }

                var profilePlayers = await query.ToListAsync();

                // Map the database entities to DTOs
                var mappedProfilePlayers = profilePlayers.Select(player => new ProfilesDTO.ProfilePlayerEvent
                {
                    ID = player.ID,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    RegionID = player.School?.SchoolDivision?.SchoolRegion?.ID,
                    SubCategoryID = player.ProfilePlayerSports.FirstOrDefault()?.SportSubcategory?.ID ?? 0
                }).ToList();

                return Ok(mappedProfilePlayers);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpGet("Player/FilteredPlayer")]
        public async Task<ActionResult<List<ProfilesDTO.ProfilePlayerEvent>>> GetProfilePlayerEvent(
            [FromQuery] int? regionID,
            [FromQuery] int? sportID,
            [FromQuery] int? schoolLevelID,
            [FromQuery] int? genderID,
            [FromQuery] int? subCategoryID)
        {
            try
            {
                var query = _context.ProfilePlayers
                    .Include(p => p.School)
                    .ThenInclude(sd => sd!.SchoolDivision)
                    .ThenInclude(sr => sr!.SchoolRegion)
                    .Include(p => p.ProfilePlayerSports)
                    .ThenInclude(ppsc => ppsc.SportSubcategory)
                    .AsQueryable();

                // Apply filters if provided
                if (regionID.HasValue)
                {
                    query = query.Where(p => p.School!.SchoolDivision!.SchoolRegion!.ID == regionID.Value);
                }

                if (sportID.HasValue)
                {
                    query = query.Where(p => p.SportID == sportID.Value);
                }

                if (subCategoryID.HasValue)
                {
                    query = query.Where(p => p.ProfilePlayerSports
                        .Any(ppsc => ppsc.SportSubcategory!.ID == subCategoryID.Value));
                }

                // Additional filters for school level and gender
                if (schoolLevelID.HasValue || genderID.HasValue)
                {
                    query = query.Where(p => p.ProfilePlayerSports.Any(ppsc =>
                        (!schoolLevelID.HasValue || ppsc.SportSubcategory!.SchoolLevelID == schoolLevelID.Value) &&
                        (!genderID.HasValue || ppsc.SportSubcategory!.SportGenderCategoryID == genderID.Value)
                    ));
                }

                var profilePlayers = await query.ToListAsync();

                var mappedProfilePlayers = new List<ProfilesDTO.ProfilePlayerEvent>();

                foreach (var player in profilePlayers)
                {
                    foreach (var playerSport in player.ProfilePlayerSports)
                    {
                        if (playerSport.SportSubcategory != null)
                        {
                            mappedProfilePlayers.Add(new ProfilesDTO.ProfilePlayerEvent
                            {
                                ID = player.ID,
                                FirstName = player.FirstName,
                                LastName = player.LastName,
                                RegionID = player.School?.SchoolDivision?.SchoolRegion?.ID,
                                SubCategoryID = playerSport.SportSubcategory.ID 
                            });
                        }
                    }
                }

                return Ok(mappedProfilePlayers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // Profile Players Details view
        [HttpGet("Player/Details")] // /api/Profiles/Player/Details
        public async Task<ActionResult<List<ProfilesDTO.ProfilePlayersDetails.ProfilePlayers>>> GetProfilePlayerDetails(
            [FromQuery] string? id,
            [FromQuery] string? firstName,
            [FromQuery] string? lastName,
            [FromQuery] string? school,
            [FromQuery] int? schoolLevelID,
            [FromQuery] string? level,
            [FromQuery] string? division,
            [FromQuery] int? regionID,
            [FromQuery] string? region,
            [FromQuery] string? abbreviation,
            [FromQuery] string? category,
            [FromQuery] int? sportID,
            [FromQuery] string? sport,
            [FromQuery] string? middleInitial,
            [FromQuery] string? sex,
            [FromQuery] DateTime? birthDate,
            [FromQuery] string? lrn)
        {
            try
            {
                var query = _context.ProfilePlayers
                    .Include(p => p.School)
                    .ThenInclude(sd => sd!.SchoolDivision)
                    .ThenInclude(sr => sr!.SchoolRegion)
                    .Include(p => p.School)
                    .ThenInclude(sl => sl!.SchoolLevels)
                    .Include(p => p.Sport)
                    .ThenInclude(sc => sc!.SportCategory)
                    .Include(p => p.ProfilePlayerSports)
                    .ThenInclude(ppsc => ppsc.SportSubcategory)
                    .ThenInclude(ssc => ssc!.Sport)
                    .ThenInclude(sc => sc!.SportCategory)
                    .Include(p => p.ProfilePlayerSports)
                    .ThenInclude(ppsc => ppsc.SportSubcategory)
                    .ThenInclude(sg => sg!.SportGenderCategory)
                    .Include(p => p.ProfilePlayerSports)
                    .ThenInclude(ppc => ppc.ProfilePlayerSportCoaches)
                    .ThenInclude(pc => pc.ProfileCoach)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(id))
                    query = query.Where(p => p.ID == id);

                if (!string.IsNullOrEmpty(firstName))
                    query = query.Where(p => p.FirstName != null && p.FirstName.Contains(firstName));

                if (!string.IsNullOrEmpty(lastName))
                    query = query.Where(p => p.LastName != null && p.LastName.Contains(lastName));

                if (!string.IsNullOrEmpty(middleInitial))
                    query = query.Where(p => p.MiddleInitial != null && p.MiddleInitial.Contains(middleInitial));

                if (!string.IsNullOrEmpty(sex))
                    query = query.Where(p => p.Sex != null && p.Sex.Contains(sex));

                if (birthDate.HasValue)
                    query = query.Where(p => p.BirthDate == birthDate.Value);

                if (!string.IsNullOrEmpty(lrn))
                    query = query.Where(p => p.LRN != null && p.LRN.Contains(lrn));

                if (!string.IsNullOrEmpty(school))
                    query = query.Where(p =>
                        p.School != null && p.School.School != null && p.School.School.Contains(school));

                if (schoolLevelID.HasValue)
                    query = query.Where(p =>
                        p.School != null && p.School.SchoolLevels != null &&
                        p.School.SchoolLevels.ID == schoolLevelID.Value);

                if (!string.IsNullOrEmpty(level))
                    query = query.Where(p =>
                        p.School != null && p.School.SchoolLevels != null && p.School.SchoolLevels.Level != null &&
                        p.School.SchoolLevels.Level.Contains(level));

                if (!string.IsNullOrEmpty(division))
                    query = query.Where(p =>
                        p.School != null && p.School.SchoolDivision != null &&
                        p.School.SchoolDivision.Division != null &&
                        p.School.SchoolDivision.Division.Contains(division));

                if (regionID.HasValue)
                    query = query.Where(p =>
                        p.School != null && p.School.SchoolDivision != null &&
                        p.School.SchoolDivision.SchoolRegion != null &&
                        p.School.SchoolDivision.SchoolRegion.ID == regionID.Value);

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(p =>
                        p.School != null && p.School.SchoolDivision != null &&
                        p.School.SchoolDivision.SchoolRegion != null &&
                        p.School.SchoolDivision.SchoolRegion.Region != null &&
                        p.School.SchoolDivision.SchoolRegion.Region.Contains(region));

                if (!string.IsNullOrEmpty(abbreviation))
                    query = query.Where(p =>
                        p.School != null && p.School.SchoolDivision != null &&
                        p.School.SchoolDivision.SchoolRegion != null &&
                        p.School.SchoolDivision.SchoolRegion.Abbreviation != null &&
                        p.School.SchoolDivision.SchoolRegion.Abbreviation.Contains(abbreviation));

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(p =>
                        p.Sport != null && p.Sport.SportCategory != null && p.Sport.SportCategory.Category != null &&
                        p.Sport.SportCategory.Category.Contains(category));

                if (sportID.HasValue)
                    query = query.Where(p => p.SportID == sportID.Value);

                if (!string.IsNullOrEmpty(sport))
                    query = query.Where(p => p.Sport != null && p.Sport.Sport != null && p.Sport.Sport.Contains(sport));

                var profilePlayers = await query.ToListAsync();

                var mappedProfilePlayers = profilePlayers.Select(player =>
                    new ProfilesDTO.ProfilePlayersDetails.ProfilePlayers
                    {
                        ID = player.ID,
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        MiddleInitial = player.MiddleInitial,
                        Sex = player.Sex,
                        BirthDate = player.BirthDate,
                        LRN = player.LRN,
                        School = player.School?.School,
                        SchoolLevelID = player.School?.SchoolLevels?.ID,
                        Level = player.School?.SchoolLevels?.Level,
                        Division = player.School?.SchoolDivision?.Division,
                        RegionID = player.School?.SchoolDivision?.SchoolRegion?.ID,
                        Region = player.School?.SchoolDivision?.SchoolRegion?.Region,
                        Abbreviation = player.School?.SchoolDivision?.SchoolRegion?.Abbreviation,
                        Category = player.Sport?.SportCategory?.Category,
                        SportID = player.SportID,
                        Sport = player.Sport?.Sport,
                        ProfilePlayerSportsList = player.ProfilePlayerSports
                            .Select(s => new ProfilesDTO.ProfilePlayersDetails.ProfilePlayerSports
                            {
                                ProfilePlayerSportID = s.ID,
                                Subcategory = s.SportSubcategory?.Subcategory,
                                Gender = s.SportSubcategory?.SportGenderCategory?.Gender,
                                ProfilePlayerSportCoachesList = s.ProfilePlayerSportCoaches
                                    .Where(pc => pc.ProfileCoach != null)
                                    .Select(pc => new ProfilesDTO.ProfilePlayersDetails.ProfilePlayerSportCoaches
                                    {
                                        CoachFirstName = pc.ProfileCoach!.FirstName,
                                        CoachLastName = pc.ProfileCoach!.LastName
                                    }).ToList()
                            }).ToList()
                    }).ToList();

                return Ok(mappedProfilePlayers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}