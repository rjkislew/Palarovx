using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public EventsController(Palaro2026Context context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Event Views

        [HttpGet("Details")]
        public async Task<ActionResult<List<EventsDTO.EventDetails.Event>>> GetEventDetails(
        [FromQuery] string? id = null,
        [FromQuery] string? region = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? category = null,
        [FromQuery] string? sport = null,
        [FromQuery] string? subcategory = null,
        [FromQuery] string? gender = null,
        [FromQuery] string? level = null,
        [FromQuery] string? venue = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool? onStream = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] string? eventStage = null,
        [FromQuery] string? streamService = null,
        [FromQuery] string? userID = null)
        {
            try
            {
                var query = _context.Events
                    .Include(e => e.SportSubcategory)
                        .ThenInclude(ssc => ssc!.SportGenderCategory)
                    .Include(e => e.SportSubcategory)
                        .ThenInclude(ssc => ssc!.Sport)
                            .ThenInclude(s => s!.SportCategory)
                    .Include(e => e.SportSubcategory)
                        .ThenInclude(ssc => ssc!.SchoolLevel)
                    .Include(e => e.EventVenues)
                    .Include(e => e.EventStage)
                    .Include(e => e.EventStream)
                        .ThenInclude(e => e!.EventStreamService)
                    .Include(e => e.EventVersusTeams)
                        .ThenInclude(ev => ev.SchoolRegion)
                    .Include(e => e.EventVersusTeams)
                        .ThenInclude(e => e.EventVersusTeamPlayers)
                            .ThenInclude(e => e.ProfilePlayer)
                                .ThenInclude(e => e!.School)
                    .Include(u => u.User)
                    .AsQueryable();

                // Helper to parse comma-separated strings
                List<string> ParseCsv(string? input) =>
                    string.IsNullOrWhiteSpace(input) ? [] : input.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                var sportList = ParseCsv(sport);
                var eventStageList = ParseCsv(eventStage);
                var streamServiceList = ParseCsv(streamService);
                var regionList = ParseCsv(region);

                // Inline filters
                if (!string.IsNullOrEmpty(id))
                    query = query.Where(e => e.ID! == id);

                if (!string.IsNullOrEmpty(firstName))
                    query = query.Where(e =>
                        e.EventVersusTeams!
                            .SelectMany(ev => ev.EventVersusTeamPlayers!)
                            .Any(p => p.ProfilePlayer != null && p.ProfilePlayer.FirstName!.Contains(firstName)));

                if (!string.IsNullOrEmpty(lastName))
                    query = query.Where(e =>
                        e.EventVersusTeams!
                            .SelectMany(ev => ev.EventVersusTeamPlayers!)
                            .Any(p => p.ProfilePlayer != null && p.ProfilePlayer.LastName!.Contains(lastName)));

                if (regionList.Any())
                    query = query.Where(e => e.EventVersusTeams!.Any(ev => ev.SchoolRegion != null && regionList.Contains(ev.SchoolRegion.Region!)));

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(e => e.SportSubcategory!.Sport!.SportCategory!.Category == category);

                if (sportList.Any())
                    query = query.Where(e => sportList.Contains(e.SportSubcategory!.Sport!.Sport!));

                if (!string.IsNullOrEmpty(subcategory))
                    query = query.Where(e => e.SportSubcategory!.Subcategory == subcategory);

                if (!string.IsNullOrEmpty(gender))
                    query = query.Where(e => e.SportSubcategory!.SportGenderCategory!.Gender == gender);

                if (!string.IsNullOrEmpty(level))
                    query = query.Where(e => e.SportSubcategory!.SchoolLevel!.Level == level);

                if (!string.IsNullOrEmpty(venue))
                    query = query.Where(e => e.EventVenues!.Venue! == venue);

                if (eventStageList.Any())
                    query = query.Where(e => eventStageList.Contains(e.EventStage!.Stage!));

                if (streamServiceList.Any())
                    query = query.Where(e => streamServiceList.Contains(e.EventStream!.EventStreamService!.StreamService!));

                if (startDate.HasValue && endDate.HasValue)
                    query = query.Where(e => e.Date >= startDate.Value.Date && e.Date <= endDate.Value.Date);
                else if (startDate.HasValue)
                    query = query.Where(e => e.Date >= startDate.Value.Date);
                else if (endDate.HasValue)
                    query = query.Where(e => e.Date <= endDate.Value.Date);

                if (onStream.HasValue)
                    query = query.Where(e => e.OnStream == onStream.Value);

                if (isFinished.HasValue)
                    query = query.Where(e => e.IsFinished == isFinished.Value);

                if (!string.IsNullOrEmpty(userID))
                    query = query.Where(e => e.UserID == userID);

                // Execute
                var eventEntities = await query.AsNoTracking().ToListAsync();

                // Map to DTO
                var eventDTO = eventEntities.Select(eventEntity => new EventsDTO.EventDetails.Event
                {
                    ID = eventEntity.ID,
                    EventStage = eventEntity.EventStage?.Stage,
                    EventVersusList = eventEntity.EventVersusTeams?
                        .GroupBy(ev => new
                        {
                            ev.ID,
                            ev.Score,
                            ev.SchoolRegion?.Region,
                            ev.SchoolRegion?.Abbreviation,
                            ev.Rank,
                            ev.RecentUpdateAt
                        })
                        .Select(evGroup => new EventsDTO.EventDetails.EventVersusTeams
                        {
                            ID = evGroup.Key.ID,
                            Score = evGroup.Key.Score,
                            Region = evGroup.Key.Region,
                            Abbreviation = evGroup.Key.Abbreviation,
                            Rank = evGroup.Key.Rank,
                            RecentUpdateAt = evGroup.Key.RecentUpdateAt,
                            EventVersusTeamPlayersList = evGroup
                                .SelectMany(ev => ev.EventVersusTeamPlayers ?? new List<EventVersusTeamPlayers>())
                                .Select(player => new EventsDTO.EventDetails.EventVersusTeamPlayers
                                {
                                    ID = player.ID,
                                    EventVersusID = player.EventVersusID,
                                    FirstName = player.ProfilePlayer?.FirstName,
                                    LastName = player.ProfilePlayer?.LastName,
                                    School = player.ProfilePlayer?.School?.School
                                }).ToList()
                        }).ToList() ?? new List<EventsDTO.EventDetails.EventVersusTeams>(),

                    Category = eventEntity.SportSubcategory?.Sport?.SportCategory?.Category,
                    Sport = eventEntity.SportSubcategory?.Sport?.Sport,
                    SubCategoryID = eventEntity.SportSubcategory?.ID,
                    Subcategory = eventEntity.SportSubcategory?.Subcategory,
                    Gender = eventEntity.SportSubcategory?.SportGenderCategory?.Gender,
                    Level = eventEntity.SportSubcategory?.SchoolLevel?.Level,
                    Venue = eventEntity.EventVenues?.Venue,
                    Latitude = eventEntity.EventVenues?.Latitude ?? 0,
                    Longitude = eventEntity.EventVenues?.Longitude ?? 0,
                    Date = eventEntity.Date,
                    Time = eventEntity.Time,
                    OnStream = eventEntity.OnStream ?? false,
                    StreamService = eventEntity.EventStream?.EventStreamService?.StreamService,
                    StreamTitle = eventEntity.EventStream?.StreamTitle,
                    StreamURL = eventEntity.EventStream?.StreamURL,
                    IsFinished = eventEntity.IsFinished,
                    Attachement = eventEntity.Attachement,
                    Archived = eventEntity.Archived,
                    Deleted = eventEntity.Deleted,
                    FirstName = eventEntity.User?.FirstName,
                    LastName = eventEntity.User?.LastName,
                }).ToList();

                return Ok(eventDTO);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }


        // ------------------------------------------------------------------------------------------------------------------

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

        // Event Stream Services

        // View Event Stream Services with details
        [HttpGet("StreamServices/Details")]
        public async Task<ActionResult<List<EventsDTO.EventStreamServicesDetails.EventStreamServices>>> GetEventStreamServicesDetails()
        {
            try
            {
                var eventStreamServices = await _context.EventStreamServices
                    .Include(es => es.EventStreams)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedEventStreamServices = eventStreamServices.Select(streamService => new EventsDTO.EventStreamServicesDetails.EventStreamServices
                {
                    ID = streamService.ID,
                    StreamService = streamService.StreamService,
                    EventStreamsList = streamService.EventStreams.Select(stream => new EventsDTO.EventStreamServicesDetails.EventStreams
                    {
                        StreamID = stream.ID,
                        StreamTitle = stream.StreamTitle,
                        StreamURL = stream.StreamURL,
                        StreamDate = stream.StreamDate
                    }).ToList()
                }).ToList();

                return Ok(mappedEventStreamServices);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // Map EventStreamServices entity to EventsDTO.EventStreamServices
        private static EventsDTO.EventStreamServices EventStreamServicesDTOMapper(EventStreamServices eventStreamServices) =>
            new EventsDTO.EventStreamServices
            {
                ID = eventStreamServices.ID,
                StreamService = eventStreamServices.StreamService
            };

        [HttpGet("StreamServices")] // /api/Events/StreamServices
        public async Task<ActionResult<IEnumerable<EventsDTO.EventStreamServices>>> GetEventStreamServices(
            [FromQuery] int? ID = null,
            [FromQuery] string? streamService = null)
        {
            var query = _context.EventStreamServices.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(streamService))
                query = query.Where(x => x.StreamService!.Contains(streamService));

            return await query
                .Select(x => EventStreamServicesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("StreamServices")] // /api/Events/StreamServices
        public async Task<ActionResult<EventStreamServices>> PostEventStreamService(EventsDTO.EventStreamServices eventStreamServiceDto)
        {
            var eventStreamServiceEntity = new EventStreamServices
            {
                ID = eventStreamServiceDto.ID,
                StreamService = eventStreamServiceDto.StreamService
            };

            _context.EventStreamServices.Add(eventStreamServiceEntity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventStreamServiceExist(eventStreamServiceDto.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventStreamServices", new { id = eventStreamServiceDto.ID }, EventStreamServicesDTOMapper(eventStreamServiceEntity));
        }

        [HttpPut("StreamServices/{id}")] // /api/Events/StreamServices/{id}
        public async Task<IActionResult> PutEventStreamService(int id, EventsDTO.EventStreamServices eventStreamServiceDto)
        {
            if (id != eventStreamServiceDto.ID)
            {
                return BadRequest();
            }

            var existingStreamService = await _context.EventStreamServices.FindAsync(id);
            if (existingStreamService == null)
            {
                return NotFound();
            }

            existingStreamService.StreamService = eventStreamServiceDto.StreamService;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStreamServiceExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("StreamServices/{id}")] // /api/Events/StreamServices/{id}
        public async Task<IActionResult> PatchEventStreamService(int id, [FromBody] EventsDTO.EventStreamServices updatedService)
        {
            var existingService = await _context.EventStreamServices.FindAsync(id);

            if (existingService == null) return NotFound();

            if (updatedService.StreamService != null) existingService.StreamService = updatedService.StreamService;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStreamServiceExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventStreamServiceExist(updatedService.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("StreamServices/{id}")] // /api/Events/StreamServices/{id}
        public async Task<IActionResult> DeleteEventStreamService(int id)
        {
            var eventStreamService = await _context.EventStreamServices.FindAsync(id);
            if (eventStreamService == null)
            {
                return NotFound();
            }

            _context.EventStreamServices.Remove(eventStreamService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event Stream Service exists
        private bool EventStreamServiceExist(int id)
        {
            return _context.EventStreamServices.Any(e => e.ID == id);
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Event Streams REST methods

        // Map EventStreams entity to EventsDTO.EventStreams
        private static EventsDTO.EventStreams EventStreamsDTOMapper(EventStreams eventStreams) =>
        new EventsDTO.EventStreams
        {
            ID = eventStreams.ID,
            EventStreamServiceID = eventStreams.EventStreamServiceID,
            StreamTitle = eventStreams.StreamTitle,
            StreamDate = eventStreams.StreamDate,
            StreamURL = eventStreams.StreamURL
        };

        [HttpGet("StreamService/Streams")] // /api/Events/StreamService/Streams
        public async Task<ActionResult<IEnumerable<EventsDTO.EventStreams>>> GetEventStreams(
            [FromQuery] int? ID = null,
            [FromQuery] int? eventStreamServiceID = null,
            [FromQuery] string? streamTitle = null,
            [FromQuery] DateTime? streamDate = null,
            [FromQuery] string? streamURL = null)
        {
            var query = _context.EventStreams.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (eventStreamServiceID.HasValue)
                query = query.Where(x => x.EventStreamServiceID == eventStreamServiceID.Value);

            if (!string.IsNullOrEmpty(streamTitle))
                query = query.Where(x => x.StreamTitle!.Contains(streamTitle));

            if (streamDate.HasValue)
                query = query.Where(x => x.StreamDate == streamDate.Value);

            if (!string.IsNullOrEmpty(streamURL))
                query = query.Where(x => x.StreamURL!.Contains(streamURL));

            return await query
                .Select(x => EventStreamsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("StreamService/Streams")] // /api/Events/StreamService/Streams
        public async Task<ActionResult<EventStreams>> PostEventStreams(EventsDTO.EventStreams eventStreams)
        {
            var eventStreamsEntity = new EventStreams
            {
                ID = eventStreams.ID,
                EventStreamServiceID = eventStreams.EventStreamServiceID,
                StreamTitle = eventStreams.StreamTitle,
                StreamDate = eventStreams.StreamDate,
                StreamURL = eventStreams.StreamURL
            };

            _context.EventStreams.Add(eventStreamsEntity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventStreamsExist(eventStreams.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventStreams", new { id = eventStreams.ID }, EventStreamsDTOMapper(eventStreamsEntity));
        }

        [HttpPut("StreamService/Streams/{id}")] // /api/Events/StreamService/Streams/{id}
        public async Task<IActionResult> PutEventStreams(int id, EventsDTO.EventStreams eventStreamsDto)
        {
            if (id != eventStreamsDto.ID)
            {
                return BadRequest();
            }

            var existingEventStream = await _context.EventStreams.FindAsync(id);
            if (existingEventStream == null)
            {
                return NotFound();
            }

            existingEventStream.EventStreamServiceID = eventStreamsDto.EventStreamServiceID;
            existingEventStream.StreamTitle = eventStreamsDto.StreamTitle;
            existingEventStream.StreamDate = eventStreamsDto.StreamDate;
            existingEventStream.StreamURL = eventStreamsDto.StreamURL;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStreamsExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("StreamService/Streams/{id}")] // /api/Events/StreamService/Streams/{id}
        public async Task<IActionResult> PatchEventStreams(int id, [FromBody] EventsDTO.EventStreams updatedStream)
        {
            var existingStream = await _context.EventStreams.FindAsync(id);

            if (existingStream == null) return NotFound();

            if (updatedStream.EventStreamServiceID != null) existingStream.EventStreamServiceID = updatedStream.EventStreamServiceID;
            if (updatedStream.StreamTitle != null) existingStream.StreamTitle = updatedStream.StreamTitle;
            if (updatedStream.StreamDate != null) existingStream.StreamDate = updatedStream.StreamDate;
            if (updatedStream.StreamURL != null) existingStream.StreamURL = updatedStream.StreamURL;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStreamsExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventStreamsExist(updatedStream.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("StreamService/Streams/{id}")] // /api/Events/StreamService/Streams/{id}
        public async Task<IActionResult> DeleteEventStreams(int id)
        {
            var eventStreams = await _context.EventStreams.FindAsync(id);
            if (eventStreams == null)
            {
                return NotFound();
            }

            _context.EventStreams.Remove(eventStreams);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event Streams exists
        private bool EventStreamsExist(int id)
        {
            return _context.EventStreams.Any(e => e.ID == id);
        }

        // ------------------------------------------------------------------------------------------------------------------

        // Event Venues REST methods

        // Map EventVenues entity to EventsDTO.EventVenues
        private static EventsDTO.EventVenues EventVenuesDTOMapper(EventVenues eventVenues) =>
           new EventsDTO.EventVenues
           {
               ID = eventVenues.ID,
               Address = eventVenues.Address,
               Venue = eventVenues.Venue,
               Latitude = eventVenues.Latitude,
               Longitude = eventVenues.Longitude,
           };

        [HttpGet("Venues")] // /api/Events/Venues
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVenues>>> GetEventVenues(
        [FromQuery] int? ID = null,
        [FromQuery] string? address = null,
        [FromQuery] string? venue = null,
        [FromQuery] decimal? latitude = null,
        [FromQuery] decimal? longitude = null)
        {
            var query = _context.EventVenues.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(address))
                query = query.Where(x => x.Address!.Contains(address));

            if (!string.IsNullOrEmpty(venue))
                query = query.Where(x => x.Venue!.Contains(venue));

            if (latitude.HasValue)
                query = query.Where(x => x.Latitude == latitude.Value);

            if (longitude.HasValue)
                query = query.Where(x => x.Longitude == longitude.Value);

            return await query
                .Select(x => EventVenuesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Venues")] // /api/Events/Venues
        public async Task<ActionResult<EventVenues>> PostEventVenues(EventsDTO.EventVenues eventVenues)
        {
            var eventVenuesDTO = new EventVenues
            {
                ID = eventVenues.ID,
                Address = eventVenues.Address,
                Venue = eventVenues.Venue,
                Latitude = eventVenues.Latitude,
                Longitude = eventVenues.Longitude,
            };

            _context.EventVenues.Add(eventVenuesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventVenuesExist(eventVenues.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventVenues", new { id = eventVenues.ID }, EventVenuesDTOMapper(eventVenuesDTO));
        }

        [HttpPut("Venues/{id}")] // /api/Events/Venues/{id}
        public async Task<IActionResult> PutEventVenues(int id, EventsDTO.EventVenues eventVenuesDTO)
        {
            if (eventVenuesDTO == null || id != eventVenuesDTO.ID)
            {
                return BadRequest("Invalid venue ID or request body.");
            }

            var existingVenues = await _context.EventVenues.FindAsync(id);
            if (existingVenues == null)
            {
                return NotFound();
            }

            existingVenues.Address = eventVenuesDTO.Address;
            existingVenues.Venue = eventVenuesDTO.Venue;
            existingVenues.Latitude = eventVenuesDTO.Latitude;
            existingVenues.Longitude = eventVenuesDTO.Longitude;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVenuesExist(id)) // Make sure this checks EventVenues, not EventStreams
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

        [HttpPatch("Venues/{id}")] // /api/Events/Venues/{id}
        public async Task<IActionResult> PatchEventVenues(int id, [FromBody] EventsDTO.EventVenues updatedVenue)
        {
            var existingVenue = await _context.EventVenues.FindAsync(id);

            if (existingVenue == null) return NotFound();

            if (updatedVenue.Address != null) existingVenue.Address = updatedVenue.Address;
            if (updatedVenue.Venue != null) existingVenue.Venue = updatedVenue.Venue;
            if (updatedVenue.Latitude != null) existingVenue.Latitude = updatedVenue.Latitude;
            if (updatedVenue.Longitude != null) existingVenue.Longitude = updatedVenue.Longitude;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVenuesExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventVenuesExist(updatedVenue.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Venues/{id}")] // /api/Events/Venues/{id}
        public async Task<IActionResult> DeleteEventVenues(int id)
        {
            var eventVenues = await _context.EventVenues.FindAsync(id);
            if (eventVenues == null)
            {
                return NotFound();
            }

            _context.EventVenues.Remove(eventVenues);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event Venues exists
        private bool EventVenuesExist(int id)
        {
            return _context.EventVenues.Any(e => e.ID == id);
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
