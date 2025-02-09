using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public TeamsController(Palaro2026Context context)
        {
            _context = context;
        }

        // Team Coach
        private static TeamsDTO.TeamCoachProfiles TeamCoachProfilesDTOMapper(TeamCoachProfiles teamCoachProfiles) =>
           new TeamsDTO.TeamCoachProfiles
           {
               ID = teamCoachProfiles.ID,
               FirstName = teamCoachProfiles.FirstName,
               LastName = teamCoachProfiles.LastName,
               SchoolRegionID = teamCoachProfiles.SchoolRegionID,
               SportSubcategoryID = teamCoachProfiles.SportSubcategoryID
           };

        [HttpGet("Coach")]
        public async Task<ActionResult<IEnumerable<TeamsDTO.TeamCoachProfiles>>> GetTeamCoachProfiles(
        [FromQuery] int? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolRegionID = null,
        [FromQuery] int? sportSubcategoryID = null)
        {
            var query = _context.TeamCoachProfiles.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName.Contains(lastName));

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            if (sportSubcategoryID.HasValue)
                query = query.Where(x => x.SportSubcategoryID == sportSubcategoryID.Value);

            return await query
                .Select(x => TeamCoachProfilesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Coach/{id}")]
        public async Task<IActionResult> PutTeamCoachProfiles(int id, TeamsDTO.TeamCoachProfiles teamCoachProfiles)
        {
            if (id != teamCoachProfiles.ID)
            {
                return BadRequest();
            }

            var existingCoachProfile = await _context.TeamCoachProfiles.FindAsync(id);
            if (existingCoachProfile == null)
            {
                return NotFound();
            }

            existingCoachProfile.FirstName = teamCoachProfiles.FirstName;
            existingCoachProfile.LastName = teamCoachProfiles.LastName;
            existingCoachProfile.SchoolRegionID = teamCoachProfiles.SchoolRegionID;
            existingCoachProfile.SportSubcategoryID = teamCoachProfiles.SportSubcategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamCoachProfilesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Coach")]
        public async Task<ActionResult<TeamCoachProfiles>> PostTeamCoachProfiles(TeamsDTO.TeamCoachProfiles teamCoachProfiles)
        {
            var teamCoachProfilesDTO = new TeamCoachProfiles
            {
                ID = teamCoachProfiles.ID,
                FirstName = teamCoachProfiles.FirstName,
                LastName = teamCoachProfiles.LastName,
                SchoolRegionID = teamCoachProfiles.SchoolRegionID,
                SportSubcategoryID = teamCoachProfiles.SportSubcategoryID
            };

            _context.TeamCoachProfiles.Add(teamCoachProfilesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TeamCoachProfilesExists(teamCoachProfiles.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTeamCoachProfiles", new { id = teamCoachProfiles.ID }, TeamCoachProfilesDTOMapper(teamCoachProfilesDTO));
        }

        [HttpDelete("Coach/{id}")]
        public async Task<IActionResult> DeleteTeamCoachProfiles(int id)
        {
            var teamCoachProfiles = await _context.TeamCoachProfiles.FindAsync(id);
            if (teamCoachProfiles == null)
            {
                return NotFound();
            }

            _context.TeamCoachProfiles.Remove(teamCoachProfiles);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeamCoachProfilesExists(int id)
        {
            return _context.TeamCoachProfiles.Any(e => e.ID == id);
        }




        // Team Profiles
        private static TeamsDTO.TeamPlayerProfiles TeamPlayerProfilesDTOMapper(TeamPlayerProfiles teamPlayerProfiles) =>
           new TeamsDTO.TeamPlayerProfiles
           {
               ID = teamPlayerProfiles.ID,
               FirstName = teamPlayerProfiles.FirstName,
               LastName = teamPlayerProfiles.LastName,
               SchoolID = teamPlayerProfiles.SchoolID,
               SportSubcategoryID = teamPlayerProfiles.SportSubcategoryID
           };

        [HttpGet("Player")]
        public async Task<ActionResult<IEnumerable<TeamsDTO.TeamPlayerProfiles>>> GetTeamPlayerProfiles(
        [FromQuery] int? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolID = null,
        [FromQuery] int? sportSubcategoryID = null)
        {
            var query = _context.TeamPlayerProfiles.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName.Contains(lastName));

            if (schoolID.HasValue)
                query = query.Where(x => x.SchoolID == schoolID.Value);

            if (sportSubcategoryID.HasValue)
                query = query.Where(x => x.SportSubcategoryID == sportSubcategoryID.Value);

            return await query
                .Select(x => TeamPlayerProfilesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Player/{id}")]
        public async Task<IActionResult> PutTeamPlayerProfiles(int id, TeamsDTO.TeamPlayerProfiles teamPlayerProfiles)
        {
            if (id != teamPlayerProfiles.ID)
            {
                return BadRequest();
            }

            var existingPlayerProfile = await _context.TeamPlayerProfiles.FindAsync(id);
            if (existingPlayerProfile == null)
            {
                return NotFound();
            }

            existingPlayerProfile.FirstName = teamPlayerProfiles.FirstName;
            existingPlayerProfile.LastName = teamPlayerProfiles.LastName;
            existingPlayerProfile.SchoolID = teamPlayerProfiles.SchoolID;
            existingPlayerProfile.SportSubcategoryID = teamPlayerProfiles.SportSubcategoryID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamPlayerProfilesExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Player")]
        public async Task<ActionResult<TeamPlayerProfiles>> PostTeamPlayerProfiles(TeamsDTO.TeamPlayerProfiles teamPlayerProfiles)
        {
            var teamPlayerProfilesDTO = new TeamPlayerProfiles
            {
                ID = teamPlayerProfiles.ID,
                FirstName = teamPlayerProfiles.FirstName,
                LastName = teamPlayerProfiles.LastName,
                SchoolID = teamPlayerProfiles.SchoolID,
                SportSubcategoryID = teamPlayerProfiles.SportSubcategoryID
            };

            _context.TeamPlayerProfiles.Add(teamPlayerProfilesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TeamPlayerProfilesExists(teamPlayerProfiles.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTeamPlayerProfiles", new { id = teamPlayerProfiles.ID }, TeamPlayerProfilesDTOMapper(teamPlayerProfilesDTO));
        }

        [HttpDelete("Player/{id}")]
        public async Task<IActionResult> DeleteTeamPlayerProfiles(int id)
        {
            var teamPlayerProfiles = await _context.TeamPlayerProfiles.FindAsync(id);
            if (teamPlayerProfiles == null)
            {
                return NotFound();
            }

            _context.TeamPlayerProfiles.Remove(teamPlayerProfiles);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeamPlayerProfilesExists(int id)
        {
            return _context.TeamPlayerProfiles.Any(e => e.ID == id);
        }
    }
}
