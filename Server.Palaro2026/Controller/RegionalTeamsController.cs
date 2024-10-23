using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionalTeamsController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        // GET: api/regionalteams
        [HttpGet("Teams")]
        public async Task<ActionResult<IEnumerable<RegionalTeamsDTO.rt_RegionalTeamsDTO>>> GetRegionalTeams()
        {
            var regionalTeams = await _context.RegionalTeams
                .Select(rt => new RegionalTeamsDTO.rt_RegionalTeamsDTO
                {
                    ID = rt.ID,
                    RegionalTeamName = rt.RegionalTeamName,
                    RegionalTeamNameAbbreviation = rt.RegionalTeamNameAbbreviation
                })
                .ToListAsync();

            return Ok(regionalTeams);
        }


        // GET: api/regionalteams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegionalTeams>> GetRegionalTeam(int id)
        {
            var regionalTeam = await _context.RegionalTeams.FindAsync(id);

            if (regionalTeam == null)
            {
                return NotFound();
            }

            return regionalTeam;
        }

        // POST: api/regionalteams
        [HttpPost]
        public async Task<ActionResult<RegionalTeams>> PostRegionalTeam(RegionalTeams regionalTeam)
        {
            _context.RegionalTeams.Add(regionalTeam);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRegionalTeam", new { id = regionalTeam.ID }, regionalTeam);
        }

        // PUT: api/regionalteams/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegionalTeam(int id, RegionalTeamsDTO.rt_RegionalTeamsDTO regionalTeam)
        {
            if (id != regionalTeam.ID)
            {
                return BadRequest();
            }

            _context.Entry(regionalTeam).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegionalTeamExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/regionalteams/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegionalTeam(int id)
        {
            var regionalTeam = await _context.RegionalTeams.FindAsync(id);
            if (regionalTeam == null)
            {
                return NotFound();
            }

            _context.RegionalTeams.Remove(regionalTeam);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RegionalTeamExists(int id)
        {
            return _context.RegionalTeams.Any(e => e.ID == id);
        }

    }
}
