using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class EventsController
    {
        // Map EventVersusTeamPlayers entity to EventsDTO.EventVersusTeamPlayers
        private static EventsDTO.EventVersusTeamPlayers EventVersusTeamPlayersDTOMapper(EventVersusTeamPlayers eventVersusTeamPlayers) =>
           new EventsDTO.EventVersusTeamPlayers
           {
               ID = eventVersusTeamPlayers.ID,
               EventVersusID = eventVersusTeamPlayers.EventVersusID,
               ProfilePlayerID = eventVersusTeamPlayers.ProfilePlayerID
           };

        [HttpGet("VersusTeams/Players")] // /api/Events/VersusTeams/Players
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersusTeamPlayers>>> GetEventVersusTeamPlayers(
        [FromQuery] int? ID = null,
        [FromQuery] int? eventVersusID = null,
        [FromQuery] string? profilePlayerID = null)
        {
            var query = _context.EventVersusTeamPlayers.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (eventVersusID.HasValue)
                query = query.Where(x => x.EventVersusID == eventVersusID.Value);

            if (!string.IsNullOrEmpty(profilePlayerID))
                query = query.Where(x => x.ProfilePlayerID == profilePlayerID);

            return await query
                .Select(x => EventVersusTeamPlayersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("VersusTeams/Players")] // /api/Events/VersusTeams/Players
        public async Task<ActionResult<EventVersusTeamPlayers>> PostEventVersusTeamsPlayers([FromBody] List<EventsDTO.EventVersusTeamPlayers> eventVersusTeamPlayers)
        {
            if (eventVersusTeamPlayers == null || !eventVersusTeamPlayers.Any())
            {
                return BadRequest("No coaches provided.");
            }

            var eventVersusTeamPlayersList = eventVersusTeamPlayers.Select(player => new EventVersusTeamPlayers
            {
                EventVersusID = player.EventVersusID,
                ProfilePlayerID = player.ProfilePlayerID
            }).ToList();

            _context.EventVersusTeamPlayers.AddRange(eventVersusTeamPlayersList);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{eventVersusTeamPlayers.Count} player added successfully." });
        }

        [HttpPut("VersusTeams/Players/{id}")] // /api/Events/VersusTeams/Players/{id}
        public async Task<IActionResult> PutEventVersusTeamPlayers(int id, EventsDTO.EventVersusTeamPlayers eventVersusTeamPlayers)
        {
            if (id != eventVersusTeamPlayers.ID)
            {
                return BadRequest();
            }

            var existingEventVersusTeamPlayers = await _context.EventVersusTeamPlayers.FindAsync(id);
            if (existingEventVersusTeamPlayers == null)
            {
                return NotFound();
            }

            existingEventVersusTeamPlayers.ID = eventVersusTeamPlayers.ID;
            existingEventVersusTeamPlayers.EventVersusID = eventVersusTeamPlayers.EventVersusID;
            existingEventVersusTeamPlayers.ProfilePlayerID = eventVersusTeamPlayers.ProfilePlayerID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVersusTeamPlayersExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("VersusTeams/Players/{id}")] // /api/Events/VersusTeams/Players/{id}
        public async Task<IActionResult> PatchEventVersusTeamPlayers(int id, [FromBody] EventsDTO.EventVersusTeamPlayers updatedPlayer)
        {
            var existingPlayer = await _context.EventVersusTeamPlayers.FindAsync(id);

            if (existingPlayer == null) return NotFound();

            if (updatedPlayer.EventVersusID != null) existingPlayer.EventVersusID = updatedPlayer.EventVersusID;
            if (updatedPlayer.ProfilePlayerID != null) existingPlayer.ProfilePlayerID = updatedPlayer.ProfilePlayerID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVersusTeamPlayersExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventVersusTeamPlayersExist(updatedPlayer.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("VersusTeams/Players/{id}")] // /api/Events/VersusTeams/Players/{id}
        public async Task<IActionResult> DeleteEventVersus(int id)
        {
            var eventVersusTeamPlayers = await _context.EventVersusTeamPlayers.FindAsync(id);
            if (eventVersusTeamPlayers == null)
            {
                return NotFound();
            }

            _context.EventVersusTeamPlayers.Remove(eventVersusTeamPlayers);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Delete Event Versus Team Players by Event Versus Team ID
        [HttpDelete("VersusTeams/Players/ByEventVersusTeam/{eventVersusTeamID}")] // /api/Events/VersusTeams/Players/ByEventVersusTeam/{eventVersusTeamID}
        public async Task<IActionResult> DeleteByEventVersusTeamID(int eventVersusTeamID)
        {
            var recordsToDelete = _context.EventVersusTeamPlayers
                .Where(p => p.EventVersusID == eventVersusTeamID)
                .ToList();

            if (!recordsToDelete.Any())
            {
                return NotFound(new { Message = "No records found for the given PlayerSportID." });
            }

            _context.EventVersusTeamPlayers.RemoveRange(recordsToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{recordsToDelete.Count} records deleted successfully." });
        }

        // Check if Event Versus Team Players exists
        private bool EventVersusTeamPlayersExist(int id)
        {
            return _context.EventVersusTeamPlayers.Any(e => e.ID == id);
        }
    }
}
