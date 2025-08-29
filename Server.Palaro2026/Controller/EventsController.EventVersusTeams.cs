using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class EventsController
    {
        // Map EventVersusTeams entity to EventsDTO.EventVersusTeams
        private static EventsDTO.EventVersusTeams EventVersusTeamsDTOMapper(EventVersusTeams eventVersusTeams) =>
           new EventsDTO.EventVersusTeams
           {
               ID = eventVersusTeams.ID,
               Score = eventVersusTeams.Score,
               SchoolRegionID = eventVersusTeams.SchoolRegionID,
               EventID = eventVersusTeams.EventID,
               Rank = eventVersusTeams.Rank,
               RecentUpdateAt = eventVersusTeams.RecentUpdateAt,
           };

        [HttpGet("VersusTeams")] // /api/Events/VersusTeams
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersusTeams>>> GetEventVersusTeams(
        [FromQuery] int? ID = null,
        [FromQuery] int? schoolRegionID = null,
        [FromQuery] string? eventID = null,
        [FromQuery] string? score = null,
        [FromQuery] string? rank = null,
        [FromQuery] DateTime? recentUpdateAt = null)
        {
            var query = _context.EventVersusTeams.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(score))
                query = query.Where(x => x.EventID == score);

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            if (!string.IsNullOrEmpty(eventID))
                query = query.Where(x => x.EventID == eventID);

            if (recentUpdateAt.HasValue)
                query = query.Where(x => x.RecentUpdateAt == recentUpdateAt.Value);

            return await query
                .Select(x => EventVersusTeamsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("VersusTeams")] // /api/Events/VersusTeams
        public async Task<ActionResult<EventVersusTeams>> PostEventVersusTeams(EventsDTO.EventVersusTeams eventVersusTeams)
        {
            var eventVersusTeamsDTO = new EventVersusTeams
            {
                ID = eventVersusTeams.ID,
                Score = eventVersusTeams.Score,
                SchoolRegionID = eventVersusTeams.SchoolRegionID,
                EventID = eventVersusTeams.EventID,
                Rank = eventVersusTeams.Rank,
                RecentUpdateAt = eventVersusTeams.RecentUpdateAt,
            };

            _context.EventVersusTeams.Add(eventVersusTeamsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventVersusTeamsExist(eventVersusTeams.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventVersusTeams", new { id = eventVersusTeamsDTO.ID }, EventVersusTeamsDTOMapper(eventVersusTeamsDTO));
        }

        [HttpPut("VersusTeams/{id}")] // /api/Events/VersusTeams/{id}
        public async Task<IActionResult> PutEventVersusTeams(int id, EventsDTO.EventVersusTeams updatedEvent)
        {
            var existingEvent = await _context.EventVersusTeams.FindAsync(id);
            if (existingEvent == null) return NotFound();

            // Ignore ID from the request body
            updatedEvent.ID = existingEvent.ID;

            // Only update fields that are not null
            if (!string.IsNullOrEmpty(updatedEvent.Score))
                existingEvent.Score = updatedEvent.Score;

            if (updatedEvent.SchoolRegionID.HasValue)
                existingEvent.SchoolRegionID = updatedEvent.SchoolRegionID;

            if (!string.IsNullOrEmpty(updatedEvent.EventID))
                existingEvent.EventID = updatedEvent.EventID;

            if (!string.IsNullOrEmpty(updatedEvent.Rank))
                existingEvent.Rank = updatedEvent.Rank;

            if (updatedEvent.RecentUpdateAt.HasValue)
                existingEvent.RecentUpdateAt = updatedEvent.RecentUpdateAt;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("VersusTeams/{id}")] // /api/Events/VersusTeams/{id}
        public async Task<IActionResult> PatchEventVersusTeams(int id, [FromBody] EventsDTO.EventVersusTeams updatedVersus)
        {
            var existingVersus = await _context.EventVersusTeams.FindAsync(id);

            if (existingVersus == null) return NotFound();

            if (updatedVersus.Score != null) existingVersus.Score = updatedVersus.Score;
            if (updatedVersus.SchoolRegionID != null) existingVersus.SchoolRegionID = updatedVersus.SchoolRegionID;
            if (updatedVersus.EventID != null) existingVersus.EventID = updatedVersus.EventID;
            if (updatedVersus.Rank != null) existingVersus.Rank = updatedVersus.Rank;
            if (updatedVersus.RecentUpdateAt != null) existingVersus.RecentUpdateAt = updatedVersus.RecentUpdateAt;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVersusTeamsExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventVersusTeamsExist(updatedVersus.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("VersusTeams/{id}")] // /api/Events/VersusTeams/{id}
        public async Task<IActionResult> DeleteEventVersusTeams(int id)
        {
            var eventVersusTeams = await _context.EventVersusTeams.FindAsync(id);
            if (eventVersusTeams == null)
            {
                return NotFound();
            }

            _context.EventVersusTeams.Remove(eventVersusTeams);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event Versus Teams exists
        private bool EventVersusTeamsExist(int id)
        {
            return _context.EventVersusTeams.Any(e => e.ID == id);
        }
    }
}
