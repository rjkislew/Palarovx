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
    public class ProfilesController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public ProfilesController(Palaro2026Context context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------------------------------------------------------

        // view coaches details

        [HttpGet("Coach/Details")] // /api/Profiles/Coach/Details
        public async Task<ActionResult<List<ProfilesDTO.ProfileCoachesDetails>>> GetProfileCoachesDetails()
        {
            try
            {
                var profileCoaches = await _context.ProfileCoaches
                    .Include(sr => sr.SchoolRegion)
                    .AsNoTracking()
                    .ToListAsync();

                var profileCoachesDTO = profileCoaches.Select(profileCoach => new ProfilesDTO.ProfileCoachesDetails
                {
                    ID = profileCoach.ID,
                    FirstName = profileCoach.FirstName,
                    LastName = profileCoach.LastName,
                    Region = profileCoach.SchoolRegion?.Region,
                    Abbreviation = profileCoach.SchoolRegion?.Abbreviation,
                }).ToList();

                return Ok(profileCoachesDTO);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Profile Coachs REST methods

        // Map ProfileCoaches to ProfilesDTO.ProfileCoaches
        private static ProfilesDTO.ProfileCoaches ProfileCoachesDTOMapper(ProfileCoaches profileCoaches) =>
           new ProfilesDTO.ProfileCoaches
           {
               ID = profileCoaches.ID,
               FirstName = profileCoaches.FirstName,
               LastName = profileCoaches.LastName,
               SchoolRegionID = profileCoaches.SchoolRegionID,
           };

        [HttpGet("Coach")] // /api/Profiles/Coach
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfileCoaches>>> GetProfileCoaches(
        [FromQuery] string? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolRegionID = null)
        {
            var query = _context.ProfileCoaches.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName!.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName!.Contains(lastName));

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            return await query
                .Select(x => ProfileCoachesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Coach")] // /api/Profiles/Coach
        public async Task<ActionResult<ProfileCoaches>> PostProfileCoaches(ProfilesDTO.ProfileCoaches profileCoaches)
        {
            var profileCoachesDTO = new ProfileCoaches
            {
                ID = profileCoaches.ID,
                FirstName = profileCoaches.FirstName,
                LastName = profileCoaches.LastName,
                SchoolRegionID = profileCoaches.SchoolRegionID,
            };

            _context.ProfileCoaches.Add(profileCoachesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfileCoachesExist(profileCoaches.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfileCoaches", new { id = profileCoaches.ID }, ProfileCoachesDTOMapper(profileCoachesDTO));
        }

        [HttpPut("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> PutProfileCoaches(string id, ProfilesDTO.ProfileCoaches profileCoaches)
        {
            if (id != profileCoaches.ID)
            {
                return BadRequest();
            }

            var existingCoachProfile = await _context.ProfileCoaches.FindAsync(id);
            if (existingCoachProfile == null)
            {
                return NotFound();
            }

            existingCoachProfile.FirstName = profileCoaches.FirstName;
            existingCoachProfile.LastName = profileCoaches.LastName;
            existingCoachProfile.SchoolRegionID = profileCoaches.SchoolRegionID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileCoachesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> PatchProfileCoaches(string id, [FromBody] ProfilesDTO.ProfileCoaches updatedProfileCoach)
        {
            var existingCoach = await _context.ProfileCoaches.FindAsync(id);

            if (existingCoach == null) return NotFound();

            if (updatedProfileCoach.FirstName != null) existingCoach.FirstName = updatedProfileCoach.FirstName;
            if (updatedProfileCoach.LastName != null) existingCoach.LastName = updatedProfileCoach.LastName;
            if (updatedProfileCoach.SchoolRegionID != null) existingCoach.SchoolRegionID = updatedProfileCoach.SchoolRegionID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileCoachesExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfileCoachesExist(updatedProfileCoach.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> DeleteProfileCoaches(int id)
        {
            var ProfileCoaches = await _context.ProfileCoaches.FindAsync(id);
            if (ProfileCoaches == null)
            {
                return NotFound();
            }

            _context.ProfileCoaches.Remove(ProfileCoaches);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a ProfileCoach exist by ID
        private bool ProfileCoachesExist(string id)
        {
            return _context.ProfileCoaches.Any(e => e.ID == id);
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
        [FromQuery] string? sport)
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

                if (!string.IsNullOrEmpty(school))
                    query = query.Where(p => p.School != null && p.School.School != null && p.School.School.Contains(school));

                if (schoolLevelID.HasValue)
                    query = query.Where(p => p.School != null && p.School.SchoolLevels != null && p.School.SchoolLevels.ID == schoolLevelID.Value);

                if (!string.IsNullOrEmpty(level))
                    query = query.Where(p => p.School != null && p.School.SchoolLevels != null && p.School.SchoolLevels.Level != null && p.School.SchoolLevels.Level.Contains(level));

                if (!string.IsNullOrEmpty(division))
                    query = query.Where(p => p.School != null && p.School.SchoolDivision != null && p.School.SchoolDivision.Division != null && p.School.SchoolDivision.Division.Contains(division));

                if (regionID.HasValue)
                    query = query.Where(p => p.School != null && p.School.SchoolDivision != null && p.School.SchoolDivision.SchoolRegion != null && p.School.SchoolDivision.SchoolRegion.ID == regionID.Value);

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(p => p.School != null && p.School.SchoolDivision != null && p.School.SchoolDivision.SchoolRegion != null && p.School.SchoolDivision.SchoolRegion.Region != null && p.School.SchoolDivision.SchoolRegion.Region.Contains(region));

                if (!string.IsNullOrEmpty(abbreviation))
                    query = query.Where(p => p.School != null && p.School.SchoolDivision != null && p.School.SchoolDivision.SchoolRegion != null && p.School.SchoolDivision.SchoolRegion.Abbreviation != null && p.School.SchoolDivision.SchoolRegion.Abbreviation.Contains(abbreviation));

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(p => p.Sport != null && p.Sport.SportCategory != null && p.Sport.SportCategory.Category != null && p.Sport.SportCategory.Category.Contains(category));

                if (sportID.HasValue)
                    query = query.Where(p => p.SportID == sportID.Value);

                if (!string.IsNullOrEmpty(sport))
                    query = query.Where(p => p.Sport != null && p.Sport.Sport != null && p.Sport.Sport.Contains(sport));

                var profilePlayers = await query.ToListAsync();

                var mappedProfilePlayers = profilePlayers.Select(player => new ProfilesDTO.ProfilePlayersDetails.ProfilePlayers
                {
                    ID = player.ID,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
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
                        .GroupBy(s => new { s.ID, s.SportSubcategory?.Subcategory, s.SportSubcategory?.SportGenderCategory?.Gender })
                        .Select(sport => new ProfilesDTO.ProfilePlayersDetails.ProfilePlayerSports
                        {
                            ProfilePlayerSportID = sport.Key.ID,
                            Subcategory = sport.Key.Subcategory,
                            Gender = sport.Key.Gender,
                            ProfilePlayerSportCoachesList = sport
                                .SelectMany(sportEntry => sportEntry.ProfilePlayerSportCoaches)
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
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // Profile Players REST methods

        // Map ProfilePlayers to ProfilesDTO.ProfilePlayers
        private static ProfilesDTO.ProfilePlayers ProfilePlayersDTOMapper(ProfilePlayers profilePlayers) =>
           new ProfilesDTO.ProfilePlayers
           {
               ID = profilePlayers.ID,
               FirstName = profilePlayers.FirstName,
               LastName = profilePlayers.LastName,
               SchoolID = profilePlayers.SchoolID,
               SportID = profilePlayers.SportID,
           };

        [HttpGet("Player")] // /api/Profiles/Player
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayers>>> GetProfilePlayers(
        [FromQuery] string? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolID = null,
        [FromQuery] int? sportID = null)
        {
            var query = _context.ProfilePlayers.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName!.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName!.Contains(lastName));

            if (schoolID.HasValue)
                query = query.Where(x => x.SchoolID == schoolID.Value);

            if (sportID.HasValue)
                query = query.Where(x => x.SportID == sportID.Value);


            return await query
                .Select(x => ProfilePlayersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Player")] // /api/Profiles/Player
        public async Task<ActionResult<ProfilePlayers>> PostProfilePlayers(ProfilesDTO.ProfilePlayers profilePlayers)
        {
            var profilePlayersDTO = new ProfilePlayers
            {
                ID = profilePlayers.ID,
                FirstName = profilePlayers.FirstName,
                LastName = profilePlayers.LastName,
                SchoolID = profilePlayers.SchoolID,
                SportID = profilePlayers.SportID,
            };

            _context.ProfilePlayers.Add(profilePlayersDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayersExist(profilePlayers.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfilePlayers", new { id = profilePlayers.ID }, ProfilePlayersDTOMapper(profilePlayersDTO));
        }

        [HttpPut("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> PutProfilePlayers(string id, ProfilesDTO.ProfilePlayers profilePlayers)
        {
            if (id != profilePlayers.ID)
            {
                return BadRequest();
            }

            var existingPlayerProfile = await _context.ProfilePlayers.FindAsync(id);
            if (existingPlayerProfile == null)
            {
                return NotFound();
            }

            existingPlayerProfile.FirstName = profilePlayers.FirstName;
            existingPlayerProfile.LastName = profilePlayers.LastName;
            existingPlayerProfile.SchoolID = profilePlayers.SchoolID;
            existingPlayerProfile.SportID = profilePlayers.SportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayersExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> PatchProfilePlayers(string id, [FromBody] ProfilesDTO.ProfilePlayers updatedPlayer)
        {
            var existingPlayer = await _context.ProfilePlayers.FindAsync(id);

            if (existingPlayer == null) return NotFound();

            if (updatedPlayer.FirstName != null) existingPlayer.FirstName = updatedPlayer.FirstName;
            if (updatedPlayer.LastName != null) existingPlayer.LastName = updatedPlayer.LastName;
            if (updatedPlayer.SchoolID != null) existingPlayer.SchoolID = updatedPlayer.SchoolID;
            if (updatedPlayer.SportID != null) existingPlayer.SportID = updatedPlayer.SportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayersExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayersExist(updatedPlayer.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> DeleteProfilePlayers(string id)
        {
            var ProfilePlayers = await _context.ProfilePlayers.FindAsync(id);
            if (ProfilePlayers == null)
            {
                return NotFound();
            }

            _context.ProfilePlayers.Remove(ProfilePlayers);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a ProfilePlayer exists by ID
        private bool ProfilePlayersExist(string id)
        {
            return _context.ProfilePlayers.Any(e => e.ID == id);
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Profile Player Sports

        // Map ProfilePlayerSports to ProfilesDTO.ProfilePlayerSports
        private static ProfilesDTO.ProfilePlayerSports ProfilePlayerSportsDTOMapper(ProfilePlayerSports profilePlayerSports) =>
           new ProfilesDTO.ProfilePlayerSports
           {
               ID = profilePlayerSports.ID,
               ProfilePlayerID = profilePlayerSports.ProfilePlayerID,
               SportSubcategoryID = profilePlayerSports.SportSubcategoryID,
           };

        [HttpGet("Player/Sports")] // /api/Profiles/Player/Sports
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayerSports>>> GetProfilePlayerSports(
        [FromQuery] int? ID = null,
        [FromQuery] string? profilePlayerID = null,
        [FromQuery] int? sportSubcategoryID = null)
        {
            var query = _context.ProfilePlayerSports.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(profilePlayerID))
                query = query.Where(x => x.ProfilePlayerID == profilePlayerID);

            if (sportSubcategoryID.HasValue)
                query = query.Where(x => x.SportSubcategoryID == sportSubcategoryID.Value);

            return await query
                .AsNoTracking()
                .Select(x => ProfilePlayerSportsDTOMapper(x))
                .ToListAsync();
        }

        [HttpPost("Player/Sports")] // /api/Profiles/Player/Sports
        public async Task<ActionResult<ProfilePlayerSports>> PostProfilePlayerSports(ProfilesDTO.ProfilePlayerSports profilePlayerSports)
        {
            var profilePlayerSportsDTO = new ProfilePlayerSports
            {
                ID = profilePlayerSports.ID,
                ProfilePlayerID = profilePlayerSports.ProfilePlayerID,
                SportSubcategoryID = profilePlayerSports.SportSubcategoryID,
            };

            _context.ProfilePlayerSports.Add(profilePlayerSportsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayerSportsExist(profilePlayerSports.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfilePlayerSports", new { id = profilePlayerSports.ID }, ProfilePlayerSportsDTOMapper(profilePlayerSportsDTO));
        }

        [HttpPut("Player/Sports/{id}")] // /api/Profiles/Player/Sports/{id}
        public async Task<IActionResult> PutProfilePlayerSports(int id, ProfilesDTO.ProfilePlayerSports profilePlayerSports)
        {
            if (id != profilePlayerSports.ID)
            {
                return BadRequest();
            }

            var existingProfilePlayerSport = await _context.ProfilePlayerSports.FindAsync(id);
            if (existingProfilePlayerSport == null)
            {
                return NotFound();
            }

            existingProfilePlayerSport.ProfilePlayerID = profilePlayerSports.ProfilePlayerID;
            existingProfilePlayerSport.SportSubcategoryID = profilePlayerSports.SportSubcategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayerSportsExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Player/Sports/{id}")] // /api/Profiles/Player/Sports/{id}
        public async Task<IActionResult> PatchProfilePlayerSports(int id, [FromBody] ProfilesDTO.ProfilePlayerSports updatedSport)
        {
            var existingSport = await _context.ProfilePlayerSports.FindAsync(id);

            if (existingSport == null) return NotFound();

            if (updatedSport.ProfilePlayerID != null) existingSport.ProfilePlayerID = updatedSport.ProfilePlayerID;
            if (updatedSport.SportSubcategoryID != null) existingSport.SportSubcategoryID = updatedSport.SportSubcategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayerSportsExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayerSportsExist(updatedSport.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Player/Sports/{id}")] // /api/Profiles/Player/Sports/{id}
        public async Task<IActionResult> DeleteProfilePlayerSports(int id)
        {
            var profilePlayerSports = await _context.ProfilePlayerSports.FindAsync(id);
            if (profilePlayerSports == null)
            {
                return NotFound();
            }

            _context.ProfilePlayerSports.Remove(profilePlayerSports);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Delete ProfilePlayerSports by PlayerSportID
        private bool ProfilePlayerSportsExist(int id)
        {
            return _context.ProfilePlayerSports.Any(e => e.ID == id);
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Profile Player Coaches
        private static ProfilesDTO.ProfilePlayerSportCoaches ProfilePlayerSportCoachesDTOMapper(ProfilePlayerSportCoaches profilePlayerSportCoaches) =>
           new ProfilesDTO.ProfilePlayerSportCoaches
           {
               ID = profilePlayerSportCoaches.ID,
               ProfileCoachID = profilePlayerSportCoaches.ProfileCoachID,
               ProfilePlayerSportID = profilePlayerSportCoaches.ProfilePlayerSportID
           };

        [HttpGet("Player/Sports/Coaches")] // /api/Profiles/Player/Sports/Coaches
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayerSportCoaches>>> GetProfilePlayerSportCoaches(
        [FromQuery] int? ID = null,
        [FromQuery] string? profileCoachID = null,
        [FromQuery] int? profilePlayerSportID = null)
        {
            var query = _context.ProfilePlayerSportCoaches.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(profileCoachID))
                query = query.Where(x => x.ProfileCoachID == profileCoachID);

            if (profilePlayerSportID.HasValue)
                query = query.Where(x => x.ProfilePlayerSportID == profilePlayerSportID.Value);

            return await query
                .AsNoTracking()
                .Select(x => ProfilePlayerSportCoachesDTOMapper(x))
                .ToListAsync();
        }

        [HttpPost("Player/Sports/Coaches")] // /api/Profiles/Player/Sports/Coaches
        public async Task<ActionResult> AddProfilePlayerSportCoaches([FromBody] List<ProfilesDTO.ProfilePlayerSportCoaches> coaches)
        {
            if (coaches == null || !coaches.Any())
            {
                return BadRequest("No coaches provided.");
            }

            var profilePlayerSportCoachesList = coaches.Select(coach => new ProfilePlayerSportCoaches
            {
                ProfileCoachID = coach.ProfileCoachID,
                ProfilePlayerSportID = coach.ProfilePlayerSportID
            }).ToList();

            _context.ProfilePlayerSportCoaches.AddRange(profilePlayerSportCoachesList);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{coaches.Count} coaches added successfully." });
        }

        [HttpPut("Player/Sports/Coaches/{id}")] // /api/Profiles/Player/Sports/Coaches/{id}
        public async Task<IActionResult> PutProfilePlayerSportCoaches(int id, ProfilesDTO.ProfilePlayerSportCoaches profilePlayerSportCoaches)
        {
            if (id != profilePlayerSportCoaches.ID)
            {
                return BadRequest();
            }

            var existingProfilePlayerSportCoach = await _context.ProfilePlayerSportCoaches.FindAsync(id);
            if (existingProfilePlayerSportCoach == null)
            {
                return NotFound();
            }

            existingProfilePlayerSportCoach.ProfileCoachID = profilePlayerSportCoaches.ProfileCoachID;
            existingProfilePlayerSportCoach.ProfilePlayerSportID = profilePlayerSportCoaches.ProfilePlayerSportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayerSportCoachesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Player/Sports/Coaches/{id}")] // /api/Profiles/Player/Sports/Coaches/{id}
        public async Task<IActionResult> PatchProfilePlayerSportCoaches(int id, [FromBody] ProfilesDTO.ProfilePlayerSportCoaches updatedPlayerSportCoach)
        {
            var existingCoach = await _context.ProfilePlayerSportCoaches.FindAsync(id);

            if (existingCoach == null) return NotFound();

            if (updatedPlayerSportCoach.ProfileCoachID != null) existingCoach.ProfileCoachID = updatedPlayerSportCoach.ProfileCoachID;
            if (updatedPlayerSportCoach.ProfilePlayerSportID != null) existingCoach.ProfilePlayerSportID = updatedPlayerSportCoach.ProfilePlayerSportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProfilePlayerSportCoaches.Any(e => e.ID == id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (_context.ProfilePlayerSportCoaches.Any(e => e.ID == updatedPlayerSportCoach.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Player/Sports/Coaches/{id}")] // /api/Profiles/Player/Sports/Coaches/{id}
        public async Task<IActionResult> DeleteProfilePlayerSportCoaches(int id)
        {
            var profilePlayerSportCoaches = await _context.ProfilePlayerSportCoaches.FindAsync(id);
            if (profilePlayerSportCoaches == null)
            {
                return NotFound();
            }

            _context.ProfilePlayerSportCoaches.Remove(profilePlayerSportCoaches);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Delete ProfilePlayerSportCoaches by PlayerSportID
        [HttpDelete("Player/Sports/Coaches/ByPlayerSport/{playerSportID}")] // /api/Profiles/Player/Sports/Coaches/ByPlayerSport/{playerSportID}
        public async Task<IActionResult> DeleteByProfilePlayerSportID(int playerSportID)
        {
            var recordsToDelete = _context.ProfilePlayerSportCoaches
                .Where(p => p.ProfilePlayerSportID == playerSportID)
                .ToList();

            if (!recordsToDelete.Any())
            {
                return NotFound(new { Message = "No records found for the given PlayerSportID." });
            }

            _context.ProfilePlayerSportCoaches.RemoveRange(recordsToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{recordsToDelete.Count} records deleted successfully." });
        }

        // Check if a ProfilePlayerSportCoach exists by ID
        private bool ProfilePlayerSportCoachesExist(int id)
        {
            return _context.ProfilePlayerSportCoaches.Any(e => e.ID == id);
        }
    }
}
