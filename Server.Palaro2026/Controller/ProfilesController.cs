using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public ProfilesController(Palaro2026Context context)
        {
            _context = context;
        }


        [HttpGet("Coach/Details")]
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

        // Profile Coachs
        private static ProfilesDTO.ProfileCoaches ProfileCoachesDTOMapper(ProfileCoaches profileCoaches) =>
           new ProfilesDTO.ProfileCoaches
           {
               ID = profileCoaches.ID,
               FirstName = profileCoaches.FirstName,
               LastName = profileCoaches.LastName,
               SchoolRegionID = profileCoaches.SchoolRegionID,
           };

        [HttpGet("Coach")]
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfileCoaches>>> GetProfileCoaches(
        [FromQuery] int? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolRegionID = null)
        {
            var query = _context.ProfileCoaches.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

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

        [HttpPut("Coach/{id}")]
        public async Task<IActionResult> PutProfileCoaches(int id, ProfilesDTO.ProfileCoaches profileCoaches)
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
                if (!ProfileCoachesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Coach")]
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
                if (ProfileCoachesExists(profileCoaches.ID))
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

        [HttpDelete("Coach/{id}")]
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

        private bool ProfileCoachesExists(int id)
        {
            return _context.ProfileCoaches.Any(e => e.ID == id);
        }


        [HttpGet("Player/Details")]
        public async Task<ActionResult<List<ProfilesDTO.ProfilePlayersDetails.ProfilePlayers>>> GetProfilePlayerDetails()
        {
            try
            {
                var profilePlayers = await _context.ProfilePlayers
                    .Include(p => p.School)
                        .ThenInclude(sd => sd!.SchoolDivision)
                            .ThenInclude(sr => sr!.SchoolRegion)
                    .Include(p => p.School)
                        .ThenInclude(sl => sl!.SchoolLevels) // Assuming it's a collection
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
                    .ToListAsync();

                // Map the database entities to DTOs
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
                        .GroupBy(s => new { s.ID ,s.SportSubcategory?.Subcategory, s.SportSubcategory?.SportGenderCategory?.Gender })
                        .Select(sport => new ProfilesDTO.ProfilePlayersDetails.ProfilePlayerSports
                        {
                            ProfilePlayerSportID = sport.Key.ID,
                            Subcategory = sport.Key.Subcategory,
                            Gender = sport.Key.Gender,
                            ProfilePlayerSportCoachesList = sport
                                .SelectMany(sportEntry => sportEntry.ProfilePlayerSportCoaches) // Fix: Iterate through collection
                                .Where(pc => pc.ProfileCoach != null) // Ensure non-null coaches
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


        // Profile Players
        private static ProfilesDTO.ProfilePlayers ProfilePlayersDTOMapper(ProfilePlayers profilePlayers) =>
           new ProfilesDTO.ProfilePlayers
           {
               ID = profilePlayers.ID,
               FirstName = profilePlayers.FirstName,
               LastName = profilePlayers.LastName,
               SchoolID = profilePlayers.SchoolID,
               SportID = profilePlayers.SportID,
           };

        [HttpGet("Player")]
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayers>>> GetProfilePlayers(
        [FromQuery] int? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolID = null,
        [FromQuery] int? sportID = null)
        {
            var query = _context.ProfilePlayers.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

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

        [HttpPut("Player/{id}")]
        public async Task<IActionResult> PutProfilePlayers(int id, ProfilesDTO.ProfilePlayers profilePlayers)
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
                if (!ProfilePlayersExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Player")]
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
                if (ProfilePlayersExists(profilePlayers.ID))
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

        [HttpDelete("Player/{id}")]
        public async Task<IActionResult> DeleteProfilePlayers(int id)
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

        private bool ProfilePlayersExists(int id)
        {
            return _context.ProfilePlayers.Any(e => e.ID == id);
        }




        // Profile Player Sports
        private static ProfilesDTO.ProfilePlayerSports ProfilePlayerSportsDTOMapper(ProfilePlayerSports profilePlayerSports) =>
           new ProfilesDTO.ProfilePlayerSports
           {
               ID = profilePlayerSports.ID,
               ProfilePlayerID = profilePlayerSports.ProfilePlayerID,
               SportSubcategoryID = profilePlayerSports.SportSubcategoryID,
           };

        [HttpGet("Player/Sports")]
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayerSports>>> GetProfilePlayerSports(
        [FromQuery] int? ID = null,
        [FromQuery] int? profilePlayerID = null,
        [FromQuery] int? sportSubcategoryID = null)
        {
            var query = _context.ProfilePlayerSports.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (profilePlayerID.HasValue)
                query = query.Where(x => x.ProfilePlayerID == profilePlayerID.Value);

            if (sportSubcategoryID.HasValue)
                query = query.Where(x => x.SportSubcategoryID == sportSubcategoryID.Value);

            return await query
                .AsNoTracking()
                .Select(x => ProfilePlayerSportsDTOMapper(x))
                .ToListAsync();
        }

        [HttpPut("Player/Sports/{id}")]
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
                if (!ProfilePlayerSportsExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Player/Sports")]
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
                if (ProfilePlayerSportsExists(profilePlayerSports.ID))
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

        [HttpDelete("Player/Sports/{id}")]
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

        private bool ProfilePlayerSportsExists(int id)
        {
            return _context.ProfilePlayerSports.Any(e => e.ID == id);
        }




        // Profile Player Coaches
        private static ProfilesDTO.ProfilePlayerSportCoaches ProfilePlayerSportCoachesDTOMapper(ProfilePlayerSportCoaches profilePlayerSportCoaches) =>
           new ProfilesDTO.ProfilePlayerSportCoaches
           {
               ID = profilePlayerSportCoaches.ID,
               ProfileCoachID = profilePlayerSportCoaches.ProfileCoachID,
               ProfilePlayerSportID = profilePlayerSportCoaches.ProfilePlayerSportID
           };

        [HttpGet("Player/Sports/Coaches")]
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayerSportCoaches>>> GetProfilePlayerSportCoaches(
        [FromQuery] int? ID = null,
        [FromQuery] int? profileCoachID = null,
        [FromQuery] int? profilePlayerSportID = null)
        {
            var query = _context.ProfilePlayerSportCoaches.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (profileCoachID.HasValue)
                query = query.Where(x => x.ProfileCoachID == profileCoachID.Value);

            if (profilePlayerSportID.HasValue)
                query = query.Where(x => x.ProfilePlayerSportID == profilePlayerSportID.Value);

            return await query
                .AsNoTracking()
                .Select(x => ProfilePlayerSportCoachesDTOMapper(x))
                .ToListAsync();
        }

        [HttpPut("Player/Sports/Coaches/{id}")]
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
                if (!ProfilePlayerSportCoachesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Player/Sports/Coaches")]
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


        [HttpDelete("Player/Sports/Coaches/{id}")]
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

        [HttpDelete("Player/Sports/Coaches/ByPlayerSport/{playerSportID}")]
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


        private bool ProfilePlayerSportCoachesExists(int id)
        {
            return _context.ProfilePlayerSportCoaches.Any(e => e.ID == id);
        }
    }
}
