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

        [HttpGet("Details")]
        public async Task<ActionResult<List<EventsDTO.EventDetails.Event>>> GetEventDetails(
    [FromQuery] string? region = null,
    [FromQuery] string? category = null,
    [FromQuery] string? sport = null, // Comma-separated string
    [FromQuery] string? subcategory = null,
    [FromQuery] string? gender = null,
    [FromQuery] string? level = null,
    [FromQuery] string? venue = null,
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null,
    [FromQuery] bool? onStream = null,
    [FromQuery] bool? isFinished = null,
    [FromQuery] string? eventStage = null, // Comma-separated string
    [FromQuery] string? streamService = null, // Comma-separated string
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

                // Helper function to split comma-separated values into a list
                List<string> ParseCsv(string? input) =>
                    string.IsNullOrWhiteSpace(input)
                        ? new List<string>()
                        : input.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                var sportList = ParseCsv(sport);
                var eventStageList = ParseCsv(eventStage);
                var streamServiceList = ParseCsv(streamService);
                var regionList = ParseCsv(region);

                // Apply filters

                if (eventStageList.Any())
                {
                    query = query.Where(e => eventStageList.Contains(e.EventStage!.Stage!));
                }

                if (regionList.Any())
                {
                    query = query.Where(e => e.EventVersusTeams!.Any(ev => ev.SchoolRegion != null && regionList.Contains(ev.SchoolRegion.Region!)));
                }


                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(e => e.SportSubcategory!.Sport!.SportCategory!.Category == category);
                }

                if (sportList.Any())
                {
                    query = query.Where(e => sportList.Contains(e.SportSubcategory!.Sport!.Sport!));
                }

                if (!string.IsNullOrEmpty(subcategory))
                {
                    query = query.Where(e => e.SportSubcategory!.Subcategory == subcategory);
                }

                if (!string.IsNullOrEmpty(gender))
                {
                    query = query.Where(e => e.SportSubcategory!.SportGenderCategory!.Gender == gender);
                }

                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(e => e.SportSubcategory!.SchoolLevel!.Level == level);
                }

                if (!string.IsNullOrEmpty(venue))
                {
                    query = query.Where(e => e!.EventVenues!.Venue! == venue);
                }

                if (eventStageList.Any())
                {
                    query = query.Where(e => eventStageList.Contains(e.EventStage!.Stage!));
                }

                if (streamServiceList.Any())
                {
                    query = query.Where(e => streamServiceList.Contains(e.EventStream!.EventStreamService!.StreamService!));
                }

                // Filter by StartDate and EndDate range
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(e => e.Date >= startDate.Value.Date && e.Date <= endDate.Value.Date);
                }
                else if (startDate.HasValue)
                {
                    query = query.Where(e => e.Date >= startDate.Value.Date);
                }
                else if (endDate.HasValue)
                {
                    query = query.Where(e => e.Date <= endDate.Value.Date);
                }

                if (onStream.HasValue)
                {
                    query = query.Where(e => e.OnStream == onStream.Value);
                }

                if (isFinished.HasValue)
                {
                    query = query.Where(e => e.IsFinished == isFinished.Value);
                }

                if (!string.IsNullOrEmpty(userID))
                {
                    query = query.Where(e => e.UserID == userID);
                }

                // Execute query
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
                        ev.RecentUpdateAt
                    })
                    .Select(evGroup => new EventsDTO.EventDetails.EventVersusTeams
                    {
                        ID = evGroup.Key.ID,
                        Score = evGroup.Key.Score,
                        Region = evGroup.Key.Region,
                        Abbreviation = evGroup.Key.Abbreviation,
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

        [HttpGet]
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

        [HttpPut("{id}")]
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
                if (!EventsExists(id))
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

        [HttpPost]
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
                if (EventsExists(events.ID))
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

        [HttpDelete("{id}")]
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

        private bool EventsExists(string id)
        {
            return _context.Events.Any(e => e.ID == id);
        }





        // Event Stage

        private static EventsDTO.EventStages EventStagesDTOMapper(EventStages eventStages) =>
           new EventsDTO.EventStages
           {
               ID = eventStages.ID,
               Stage = eventStages.Stage,
           };

        [HttpGet("Stages")]
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

        [HttpPut("Stages/{id}")]
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

            existingEventStages.Stage = eventStages.Stage;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStageExists(id))
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

        [HttpPost("Stages")]
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
                if (EventStageExists(eventStages.ID))
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

        [HttpDelete("Stages/{id}")]
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

        private bool EventStageExists(int id)
        {
            return _context.EventStages.Any(e => e.ID == id);
        }





        // Event News

        private static EventsDTO.EventNews EventNewsDTOMapper(EventNews eventNews) =>
           new EventsDTO.EventNews
           {
               ID = eventNews.ID,
               FacebookLink = eventNews.FacebookLink,
           };

        [HttpGet("News")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventNews>>> GetEventNews(
        [FromQuery] int? ID = null,
        [FromQuery] string? facebookLink = null)
        {
            var query = _context.EventNews.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(facebookLink))
                query = query.Where(x => x.FacebookLink!.Contains(facebookLink));

            return await query
                .Select(x => EventNewsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("News/{id}")]
        public async Task<IActionResult> PutEventNews(int id, EventsDTO.EventNews eventNews)
        {
            if (id != eventNews.ID)
            {
                return BadRequest();
            }

            var existingEventNews = await _context.EventNews.FindAsync(id);
            if (existingEventNews == null)
            {
                return NotFound();
            }

            existingEventNews.FacebookLink = eventNews.FacebookLink;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventNewsExists(id))
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

        [HttpPost("News")]
        public async Task<ActionResult<EventNews>> PostEventNews(EventsDTO.EventNews eventNews)
        {
            var eventNewsDTO = new EventNews
            {
                ID = eventNews.ID,
                FacebookLink = eventNews.FacebookLink,
            };

            _context.EventNews.Add(eventNewsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventNewsExists(eventNews.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventNews", new { id = eventNews.ID }, EventNewsDTOMapper(eventNewsDTO));
        }

        [HttpDelete("News/{id}")]
        public async Task<IActionResult> DeleteEventNews(int id)
        {
            var eventNews = await _context.EventNews.FindAsync(id);
            if (eventNews == null)
            {
                return NotFound();
            }

            _context.EventNews.Remove(eventNews);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventNewsExists(int id)
        {
            return _context.EventNews.Any(e => e.ID == id);
        }




        // Event Stream Services

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


        private static EventsDTO.EventStreamServices EventStreamServicesDTOMapper(EventStreamServices eventStreamServices) =>
            new EventsDTO.EventStreamServices
            {
                ID = eventStreamServices.ID,
                StreamService = eventStreamServices.StreamService
            };

        [HttpGet("StreamServices")]
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

        [HttpPut("StreamServices/{id}")]
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
                if (!EventStreamServiceExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("StreamServices")]
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
                if (EventStreamServiceExists(eventStreamServiceDto.ID))
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

        [HttpDelete("StreamServices/{id}")]
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

        private bool EventStreamServiceExists(int id)
        {
            return _context.EventStreamServices.Any(e => e.ID == id);
        }





        // Event Streams
        private static EventsDTO.EventStreams EventStreamsDTOMapper(EventStreams eventStreams) =>
        new EventsDTO.EventStreams
        {
            ID = eventStreams.ID,
            EventStreamServiceID = eventStreams.EventStreamServiceID,
            StreamTitle = eventStreams.StreamTitle,
            StreamDate = eventStreams.StreamDate,
            StreamURL = eventStreams.StreamURL
        };

        [HttpGet("StreamService/Streams")]
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

        [HttpPut("StreamService/Streams/{id}")]
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
                if (!EventStreamsExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("StreamService/Streams")]
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
                if (EventStreamsExists(eventStreams.ID))
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

        [HttpDelete("StreamService/Streams/{id}")]
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

        private bool EventStreamsExists(int id)
        {
            return _context.EventStreams.Any(e => e.ID == id);
        }





        // Venues

        private static EventsDTO.EventVenues EventVenuesDTOMapper(EventVenues eventVenues) =>
           new EventsDTO.EventVenues
           {
               ID = eventVenues.ID,
               Address = eventVenues.Address,
               Venue = eventVenues.Venue,
               Latitude = eventVenues.Latitude,
               Longitude = eventVenues.Longitude,
           };

        [HttpGet("Venues")]
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

        [HttpPut("Venues/{id}")]
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
                if (!EventVenuesExists(id)) // Make sure this checks EventVenues, not EventStreams
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

        [HttpPost("Venues")]
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
                if (EventVenuesExists(eventVenues.ID))
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

        [HttpDelete("Venues/{id}")]
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

        private bool EventVenuesExists(int id)
        {
            return _context.EventVenues.Any(e => e.ID == id);
        }




        // Event Versus Teams

        private static EventsDTO.EventVersusTeams EventVersusTeamsDTOMapper(EventVersusTeams eventVersusTeams) =>
           new EventsDTO.EventVersusTeams
           {
               ID = eventVersusTeams.ID,
               Score = eventVersusTeams.Score,
               SchoolRegionID = eventVersusTeams.SchoolRegionID,
               EventID = eventVersusTeams.EventID,
               RecentUpdateAt = eventVersusTeams.RecentUpdateAt,
           };

        [HttpGet("VersusTeams")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersusTeams>>> GetEventVersusTeams(
        [FromQuery] int? ID = null,
        [FromQuery] string? score = null,
        [FromQuery] int? schoolRegionID = null,
        [FromQuery] string? eventID = null,
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

        [HttpPut("VersusTeams/{id}")]
        public async Task<IActionResult> PutEventVersusTeams(int id, EventsDTO.EventVersusTeams eventVersusTeams)
        {
            if (id != eventVersusTeams.ID)
            {
                return BadRequest();
            }

            var existingEventVersusTeams = await _context.EventVersusTeams.FindAsync(id);
            if (existingEventVersusTeams == null)
            {
                return NotFound();
            }

            existingEventVersusTeams.Score = eventVersusTeams.Score;
            existingEventVersusTeams.SchoolRegionID = eventVersusTeams.SchoolRegionID;
            existingEventVersusTeams.EventID = eventVersusTeams.EventID;
            existingEventVersusTeams.RecentUpdateAt = eventVersusTeams.RecentUpdateAt;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVersusTeamsExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("VersusTeams")]
        public async Task<ActionResult<EventVersus>> PostEventVersusTeams(EventsDTO.EventVersusTeams eventVersusTeams)
        {
            var eventVersusTeamsDTO = new EventVersusTeams
            {
                ID = eventVersusTeams.ID,
                Score = eventVersusTeams.Score,
                SchoolRegionID = eventVersusTeams.SchoolRegionID,
                EventID = eventVersusTeams.EventID,
                RecentUpdateAt = eventVersusTeams.RecentUpdateAt,
            };

            _context.EventVersusTeams.Add(eventVersusTeamsDTO);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEventVersusTeams", new { id = eventVersusTeamsDTO.ID }, EventVersusTeamsDTOMapper(eventVersusTeamsDTO));
        }

        [HttpDelete("VersusTeams/{id}")]
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

        private bool EventVersusTeamsExists(int id)
        {
            return _context.EventVersusTeams.Any(e => e.ID == id);
        }




        // Event Versus Team Players

        private static EventsDTO.EventVersusTeamPlayers EventVersusTeamPlayersDTOMapper(EventVersusTeamPlayers eventVersusTeamPlayers) =>
           new EventsDTO.EventVersusTeamPlayers
           {
               ID = eventVersusTeamPlayers.ID,
               EventVersusID = eventVersusTeamPlayers.EventVersusID,
               ProfilePlayerID = eventVersusTeamPlayers.ProfilePlayerID
           };

        [HttpGet("VersusTeams/Players")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersusTeamPlayers>>> GetEventVersusTeamPlayers(
        [FromQuery] int? ID = null,
        [FromQuery] int? eventVersusID = null,
        [FromQuery] int? profilePlayerID = null)
        {
            var query = _context.EventVersusTeamPlayers.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (eventVersusID.HasValue)
                query = query.Where(x => x.EventVersusID == eventVersusID.Value);

            if (profilePlayerID.HasValue)
                query = query.Where(x => x.ProfilePlayerID == profilePlayerID.Value);

            return await query
                .Select(x => EventVersusTeamPlayersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("VersusTeams/Players/{id}")]
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
                if (!EventVersusTeamPlayersExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("VersusTeams/Players")]
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


        [HttpDelete("VersusTeams/Players/{id}")]
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

        [HttpDelete("VersusTeams/Players/ByEventVersusTeam/{eventVersusTeamID}")]
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

        private bool EventVersusTeamPlayersExists(int id)
        {
            return _context.EventVersusTeamPlayers.Any(e => e.ID == id);
        }
    }
}
