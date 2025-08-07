using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;


namespace Server.Palaro2026.Controller
{
    public partial class EventsController : ControllerBase
    {
        // Event REST methods

        // Map Events entity to EventsDTO.Events
        private static EventsDTO.Events EventsDTOMapper(Events events) =>
           new EventsDTO.Events
           {
               ID = events.ID,
               EventStageID = events.EventStageID,
               SportSubcategoryID = events.SportSubcategoryID,
               EventVenuesID = events.EventVenuesID,
               Date = events.Date,
               Time = events.Time,
               OnStream = events.OnStream,
               EventStreamID = events.EventStreamID,
               GamePhase = events.GamePhase,
               IsFinished = events.IsFinished,
               Attachement = events.Attachement,
               Archived = events.Archived,
               Deleted = events.Deleted,
               UserID = events.UserID,
           };


        [HttpGet] // /api/Events
        public async Task<ActionResult<IEnumerable<EventsDTO.Events>>> GetEvents(
        [FromQuery] string? ID = null,
        [FromQuery] int? eventStageID = null,
        [FromQuery] int? sportSubcategoryID = null,
        [FromQuery] int? eventVenuesID = null,
        [FromQuery] DateTime? date = null,
        [FromQuery] TimeSpan? time = null,
        [FromQuery] bool? onStream = null,
        [FromQuery] int? streamID = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] bool? archived = null,
        [FromQuery] bool? deleted = null,
        [FromQuery] string? userID = null)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (eventStageID.HasValue)
                query = query.Where(x => x.EventStageID == eventStageID.Value);

            if (sportSubcategoryID.HasValue)
                query = query.Where(x => x.SportSubcategoryID == sportSubcategoryID.Value);

            if (eventVenuesID.HasValue)
                query = query.Where(x => x.EventVenuesID == eventVenuesID.Value);

            if (date.HasValue)
                query = query.Where(x => x.Date == date.Value);

            if (time.HasValue)
                query = query.Where(x => x.Time == time.Value);

            if (onStream.HasValue)
                query = query.Where(x => x.OnStream == onStream.Value);

            if (streamID.HasValue)
                query = query.Where(x => x.EventStreamID == streamID.Value);

            if (isFinished.HasValue)
                query = query.Where(x => x.IsFinished == isFinished.Value);

            if (archived.HasValue)
                query = query.Where(x => x.Archived == archived.Value);

            if (deleted.HasValue)
                query = query.Where(x => x.Deleted == deleted.Value);

            if (!string.IsNullOrEmpty(userID))
                query = query.Where(x => x.UserID == userID);

            return await query
                .Select(x => EventsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost] // /api/Events
        public async Task<ActionResult<Events>> PostEvents(EventsDTO.Events events)
        {
            var eventsDTO = new Events
            {
                ID = events.ID,
                EventStageID = events.EventStageID,
                SportSubcategoryID = events.SportSubcategoryID,
                EventVenuesID = events.EventVenuesID,
                Date = events.Date,
                Time = events.Time,
                OnStream = events.OnStream,
                EventStreamID = events.EventStreamID,
                GamePhase = events.GamePhase,
                IsFinished = events.IsFinished,
                Attachement = events.Attachement,
                Archived = events.Archived,
                Deleted = events.Deleted,
                UserID = events.UserID,
            };

            _context.Events.Add(eventsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventExist(events.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEvents", new { id = events.ID }, EventsDTOMapper(eventsDTO));
        }

        [HttpPut("{id}")] // /api/Events/{id}
        public async Task<IActionResult> PutEvents(string id, EventsDTO.Events events)
        {
            if (id != events.ID)
            {
                return BadRequest();
            }

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            existingEvent.EventStageID = events.EventStageID;
            existingEvent.GamePhase = events.GamePhase;
            existingEvent.SportSubcategoryID = events.SportSubcategoryID;
            existingEvent.EventVenuesID = events.EventVenuesID;
            existingEvent.Date = events.Date;
            existingEvent.Time = events.Time;
            existingEvent.OnStream = events.OnStream;
            existingEvent.EventStreamID = events.EventStreamID;
            existingEvent.IsFinished = events.IsFinished;
            existingEvent.Attachement = events.Attachement;
            existingEvent.Archived = events.Archived;
            existingEvent.Deleted = events.Deleted;
            existingEvent.UserID = events.UserID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPatch("{id}")] // /api/Events/{id}
        public async Task<IActionResult> PatchEvents(string id, [FromBody] EventsDTO.Events updatedEvent)
        {
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null) return NotFound();

            if (updatedEvent.EventStageID != null) existingEvent.EventStageID = updatedEvent.EventStageID;
            if (updatedEvent.SportSubcategoryID != null) existingEvent.SportSubcategoryID = updatedEvent.SportSubcategoryID;
            if (updatedEvent.EventVenuesID != null) existingEvent.EventVenuesID = updatedEvent.EventVenuesID;
            if (updatedEvent.Date != null) existingEvent.Date = updatedEvent.Date;
            if (updatedEvent.Time != null) existingEvent.Time = updatedEvent.Time;
            if (updatedEvent.OnStream != null) existingEvent.OnStream = updatedEvent.OnStream;
            if (updatedEvent.EventStreamID != null) existingEvent.EventStreamID = updatedEvent.EventStreamID;
            if (updatedEvent.GamePhase != null) existingEvent.GamePhase = updatedEvent.GamePhase;
            if (updatedEvent.IsFinished != null) existingEvent.IsFinished = updatedEvent.IsFinished;
            if (updatedEvent.Attachement != null) existingEvent.Attachement = updatedEvent.Attachement;
            if (updatedEvent.Archived != null) existingEvent.Archived = updatedEvent.Archived;
            if (updatedEvent.Deleted != null) existingEvent.Deleted = updatedEvent.Deleted;
            if (updatedEvent.UserID != null) existingEvent.UserID = updatedEvent.UserID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventExist(updatedEvent.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")] // /api/Events/{id}
        public async Task<IActionResult> DeleteEvents(string id)
        {
            var events = await _context.Events.FindAsync(id);
            if (events == null)
            {
                return NotFound();
            }

            _context.Events.Remove(events);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event exists
        private bool EventExist(string id)
        {
            return _context.Events.Any(e => e.ID == id);
        }
        
        // ------------------------------------------------------------------------------------------------------------------

        // Event Stage REST methods

        // Map EventStages entity to EventsDTO.EventStages
        private static EventsDTO.EventStages EventStagesDTOMapper(EventStages eventStages) =>
           new EventsDTO.EventStages
           {
               ID = eventStages.ID,
               Stage = eventStages.Stage,
           };

        [HttpGet("Stages")] // /api/Events/Stages
        public async Task<ActionResult<IEnumerable<EventsDTO.EventStages>>> GetEventStages(
        [FromQuery] int? ID = null,
        [FromQuery] string? stage = null)
        {
            var query = _context.EventStages.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(stage))
                query = query.Where(x => x.Stage!.Contains(stage));

            return await query
                .Select(x => EventStagesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Stages")] // /api/Events/Stages
        public async Task<ActionResult<EventStages>> PostEventStages(EventsDTO.EventStages eventStages)
        {
            var eventStagesDTO = new EventStages
            {
                ID = eventStages.ID,
                Stage = eventStages.Stage,
            };

            _context.EventStages.Add(eventStagesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventStageExist(eventStages.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventStages", new { id = eventStages.ID }, EventStagesDTOMapper(eventStagesDTO));
        }

        [HttpPut("Stages/{id}")] // /api/Events/Stages/{id}
        public async Task<IActionResult> PutEventStages(int id, EventsDTO.EventStages eventStages)
        {
            if (id != eventStages.ID)
            {
                return BadRequest();
            }

            var existingEventStages = await _context.EventStages.FindAsync(id);
            if (existingEventStages == null)
            {
                return NotFound();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStageExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPatch("Stages/{id}")] // /api/Events/Stages/{id}
        public async Task<IActionResult> PatchEventStages(int id, [FromBody] EventsDTO.EventStages updatedEventStage)
        {
            var existingStage = await _context.EventStages.FindAsync(id);

            if (existingStage == null) return NotFound();

            if (updatedEventStage.Stage != null) existingStage.Stage = updatedEventStage.Stage;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStageExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventStageExist(updatedEventStage.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Stages/{id}")] // /api/Events/Stages/{id}
        public async Task<IActionResult> DeleteEventStages(int id)
        {
            var eventStages = await _context.EventStages.FindAsync(id);
            if (eventStages == null)
            {
                return NotFound();
            }

            _context.EventStages.Remove(eventStages);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event Stage exists
        private bool EventStageExist(int id)
        {
            return _context.EventStages.Any(e => e.ID == id);
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Event Versus Teams

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

        // ------------------------------------------------------------------------------------------------------------------

        // Event Versus Team Players REST methods

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
