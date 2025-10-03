using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class EventsController
    {

        // Mapper: Entity -> DTO
        private static EventsDTO.EventVersusTeamScores EventVersusTeamScoresDTOMapper(EventVersusTeamScores eventVersusScores) =>
            new EventsDTO.EventVersusTeamScores
            {
                ID = eventVersusScores.ID,
                EventID = eventVersusScores.EventID,
                SchoolRegionID = eventVersusScores.SchoolRegionID,
                PhaseNumber = eventVersusScores.PhaseNumber,
                Point = eventVersusScores.Point,
                Rank = eventVersusScores.Rank,
                RecentUpdateAt = eventVersusScores.RecentUpdateAt
            };

        // GET /api/Events/VersusTeamScores
        [HttpGet("VersusTeamScores")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersusTeamScores>>> GetEventVersusTeamScores(
            [FromQuery] int? id = null,
            [FromQuery] string? eventID = null,
            [FromQuery] int? schoolRegionID = null,
            [FromQuery] int? phaseNumber = null,
            [FromQuery] string? point = null,
            [FromQuery] string? rank = null,
            [FromQuery] DateTime? recentUpdateAt = null)
        {
            var query = _context.EventVersusTeamScores.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(eventID))
                query = query.Where(x => x.EventID == eventID);

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            if (phaseNumber.HasValue)
                query = query.Where(x => x.PhaseNumber == phaseNumber.Value);

            if (!string.IsNullOrEmpty(point))
                query = query.Where(x => x.Point == point);

            if (!string.IsNullOrEmpty(rank))
                query = query.Where(x => x.Rank == rank);

            if (recentUpdateAt.HasValue)
                query = query.Where(x => x.RecentUpdateAt == recentUpdateAt.Value);

            return await query
                .Select(x => EventVersusTeamScoresDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        // POST /api/Events/VersusTeamScores
        [HttpPost("VersusTeamScores")]
        public async Task<ActionResult<EventVersusTeamScores>> PostEventVersusTeamScores(EventsDTO.EventVersusTeamScores eventVersusScores)
        {
            var eventVersusScoresDTO = new EventVersusTeamScores
            {
                EventID = eventVersusScores.EventID,
                SchoolRegionID = eventVersusScores.SchoolRegionID,
                PhaseNumber = eventVersusScores.PhaseNumber,
                Point = eventVersusScores.Point,
                Rank = eventVersusScores.Rank,
                RecentUpdateAt = eventVersusScores.RecentUpdateAt
            };

            _context.EventVersusTeamScores.Add(eventVersusScoresDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (EventVersusTeamScoresExist(eventVersusScores.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventVersusTeamScores", new { id = eventVersusScoresDTO.ID }, EventVersusTeamScoresDTOMapper(eventVersusScoresDTO));
        }

        // PUT /api/Events/VersusTeamScores/{id}
        [HttpPut("VersusTeamScores/{id}")]
        public async Task<IActionResult> PutEventVersusTeamScores(int id, EventsDTO.EventVersusTeamScores updated)
        {
            var existing = await _context.EventVersusTeamScores.FindAsync(id);
            if (existing == null) return NotFound();

            existing.EventID = updated.EventID ?? existing.EventID;
            existing.SchoolRegionID = updated.SchoolRegionID ?? existing.SchoolRegionID;
            existing.PhaseNumber = updated.PhaseNumber != 0 ? updated.PhaseNumber : existing.PhaseNumber;
            existing.Point = updated.Point ?? existing.Point;
            existing.Rank = updated.Rank ?? existing.Rank;
            existing.RecentUpdateAt = updated.RecentUpdateAt ?? existing.RecentUpdateAt;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH /api/Events/VersusTeamScores/{id}
        [HttpPatch("VersusTeamScores/{id}")]
        public async Task<IActionResult> PatchEventVersusTeamScores(int id, [FromBody] EventVersusTeamScores updated)
        {
            var existing = await _context.EventVersusTeamScores.FindAsync(id);
            if (existing == null) return NotFound();

            if (updated.EventID != null) existing.EventID = updated.EventID;
            if (updated.SchoolRegionID.HasValue) existing.SchoolRegionID = updated.SchoolRegionID;
            if (updated.PhaseNumber != 0) existing.PhaseNumber = updated.PhaseNumber;
            if (updated.Point != null) existing.Point = updated.Point;
            if (updated.Rank != null) existing.Rank = updated.Rank;
            if (updated.RecentUpdateAt.HasValue) existing.RecentUpdateAt = updated.RecentUpdateAt;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/Events/VersusTeamScores/{id}
        [HttpDelete("VersusTeamScores/{id}")]
        public async Task<IActionResult> DeleteEventVersusTeamScores(int id)
        {
            var entity = await _context.EventVersusTeamScores.FindAsync(id);
            if (entity == null) return NotFound();

            _context.EventVersusTeamScores.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventVersusTeamScoresExist(int id)
        {
            return _context.EventVersusTeamScores.Any(e => e.ID == id);
        }
    }
}
