using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class EventTreeController : ControllerBase
    {
       

        // ========== DTO Mapper ==========
        private static EventsDTO.Event MapToDTO(Event e) =>
            new EventsDTO.Event
            {
                EventId = e.EventId,
                EventName = e.EventName,
                Date = e.Date,
                Location = e.Location,
                VersusTeams = e.EventVersusTeams?.Select(t => new EventsDTO.EventVersusTeam
                {
                    EventVersusTeamId = t.EventVersusTeamId,
                    EventId = t.EventId,
                    TeamName = t.TeamName,
                    Score = t.Score,
                    Players = t.EventVersusTeamPlayers?.Select(p => new EventsDTO.EventVersusTeamPlayer
                    {
                        EventVersusTeamPlayerId = p.EventVersusTeamPlayerId,
                        EventVersusTeamId = p.EventVersusTeamId,
                        PlayerName = p.PlayerName,
                        Position = p.Position,
                        JerseyNumber = p.JerseyNumber
                    }).ToList()
                }).ToList()
            };

        // ========== GET ALL ==========
        [HttpGet("eventstree")]
        public async Task<ActionResult<IEnumerable<EventsDTO.Event>>> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.EventVersusTeams)
                    .ThenInclude(t => t.EventVersusTeamPlayers)
                .AsNoTracking()
                .ToListAsync();

            var result = events.Select(MapToDTO).ToList();
            return Ok(result);
        }

        // ========== GET BY ID ==========
        [HttpGet("eventstree/{id}")]
        public async Task<ActionResult<EventsDTO.Event>> GetEventById(int id)
        {
            var e = await _context.Events
                .Include(ev => ev.EventVersusTeams)
                    .ThenInclude(t => t.EventVersusTeamPlayers)
                .AsNoTracking()
                .FirstOrDefaultAsync(ev => ev.EventId == id);

            if (e == null)
                return NotFound();

            return Ok(MapToDTO(e));
        }

        // ========== POST ==========
        [HttpPost("eventstree")]
        public async Task<ActionResult<EventsDTO.Event>> PostEvent(EventsDTO.Event eventDTO)
        {
            if (eventDTO == null)
                return BadRequest("Invalid event data.");

            var newEvent = new Event
            {
                EventName = eventDTO.EventName,
                Date = eventDTO.Date,
                Location = eventDTO.Location,
                EventVersusTeams = eventDTO.VersusTeams?.Select(t => new EventVersusTeam
                {
                    TeamName = t.TeamName,
                    Score = t.Score,
                    EventVersusTeamPlayers = t.Players?.Select(p => new EventVersusTeamPlayer
                    {
                        PlayerName = p.PlayerName,
                        Position = p.Position,
                        JerseyNumber = p.JerseyNumber
                    }).ToList()
                }).ToList()
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, MapToDTO(newEvent));
        }

        // ========== PUT ==========
        [HttpPut("eventstree/{id}")]
        public async Task<IActionResult> PutEvent(int id, EventsDTO.Event eventDTO)
        {
            var existing = await _context.Events
                .Include(e => e.EventVersusTeams)
                    .ThenInclude(t => t.EventVersusTeamPlayers)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existing == null)
                return NotFound();

            // Update event fields
            existing.EventName = eventDTO.EventName;
            existing.Date = eventDTO.Date;
            existing.Location = eventDTO.Location;

            // Remove old versus teams and players
            _context.EventVersusTeamPlayers.RemoveRange(existing.EventVersusTeams.SelectMany(t => t.EventVersusTeamPlayers));
            _context.EventVersusTeams.RemoveRange(existing.EventVersusTeams);

            // Add new versus teams and players
            if (eventDTO.VersusTeams != null)
            {
                existing.EventVersusTeams = eventDTO.VersusTeams.Select(t => new EventVersusTeam
                {
                    TeamName = t.TeamName,
                    Score = t.Score,
                    EventVersusTeamPlayers = t.Players?.Select(p => new EventVersusTeamPlayer
                    {
                        PlayerName = p.PlayerName,
                        Position = p.Position,
                        JerseyNumber = p.JerseyNumber
                    }).ToList()
                }).ToList();
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ========== DELETE ==========
        [HttpDelete("eventstree/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var existing = await _context.Events
                .Include(e => e.EventVersusTeams)
                    .ThenInclude(t => t.EventVersusTeamPlayers)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existing == null)
                return NotFound();

            // Remove nested data
            _context.EventVersusTeamPlayers.RemoveRange(existing.EventVersusTeams.SelectMany(t => t.EventVersusTeamPlayers));
            _context.EventVersusTeams.RemoveRange(existing.EventVersusTeams);
            _context.Events.Remove(existing);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
