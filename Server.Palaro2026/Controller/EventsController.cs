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
using static Server.Palaro2026.DTO.SchoolDTO;

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
            [FromQuery] string? sport = null,
            [FromQuery] string? subcategory = null,
            [FromQuery] string? gender = null,
            [FromQuery] string? level = null,
            [FromQuery] string? venue = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool? onStream = null,
            [FromQuery] bool? isFinished = null,
            [FromQuery] string? userID = null)
        {
            try
            {
                var query = _context.Events
                    .Include(e => e.SportSubcategory)
                        .ThenInclude(ssc => ssc.SportGenderCategory)
                    .Include(e => e.SportSubcategory)
                        .ThenInclude(ssc => ssc.Sport)
                            .ThenInclude(s => s.SportCategory)
                    .Include(e => e.SportSubcategory)
                        .ThenInclude(ssc => ssc.SchoolLevel)
                    .Include(e => e.EventVenues)
                    .Include(e => e.Stream)
                    .Include(e => e.EventVersus)
                        .ThenInclude(ev => ev.SchoolRegion)
                    .Include(u => u.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(region))
                {
                    query = query.Where(e => e.EventVersus.Any(ev => ev.SchoolRegion.Region == region));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(e => e.SportSubcategory.Sport.SportCategory.Category == category);
                }

                if (!string.IsNullOrEmpty(sport))
                {
                    query = query.Where(e => e.SportSubcategory.Sport.Sport == sport);
                }

                if (!string.IsNullOrEmpty(subcategory))
                {
                    query = query.Where(e => e.SportSubcategory.Subcategory == subcategory);
                }

                if (!string.IsNullOrEmpty(gender))
                {
                    query = query.Where(e => e.SportSubcategory.SportGenderCategory.Gender == gender);
                }

                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(e => e.SportSubcategory.SchoolLevel.Level == level);
                }

                if (!string.IsNullOrEmpty(venue))
                {
                    query = query.Where(e => e.EventVenues.Venue == venue);
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

                if (!eventEntities.Any())
                {
                    return NotFound("No events found matching the criteria.");
                }

                // Map to DTO
                var eventDTO = eventEntities.Select(eventEntity => new EventsDTO.EventDetails.Event
                {
                    ID = eventEntity.ID,
                    EventVersusList = eventEntity.EventVersus?.GroupBy(ev => new
                    {
                        ev.Score,
                        ev.SchoolRegion?.Region,
                        ev.SchoolRegion?.Abbreviation,
                        ev.RecentUpdateAt
                    })
                    .Select(ev => new EventsDTO.EventDetails.EventVersus
                    {
                        Score = ev.Key.Score,
                        Region = ev.Key.Region,
                        Abbreviation = ev.Key.Abbreviation,
                        RecentUpdateAt = ev.Key.RecentUpdateAt,
                    }).ToList() ?? new List<EventsDTO.EventDetails.EventVersus>(),

                    Category = eventEntity.SportSubcategory?.Sport?.SportCategory?.Category,
                    Sport = eventEntity.SportSubcategory?.Sport?.Sport,
                    Subcategory = eventEntity.SportSubcategory?.Subcategory,
                    Gender = eventEntity.SportSubcategory?.SportGenderCategory?.Gender,
                    Level = eventEntity.SportSubcategory?.SchoolLevel?.Level,
                    Venue = eventEntity.EventVenues?.Venue,
                    Latitude = eventEntity.EventVenues?.Latitude ?? 0,
                    Longitude = eventEntity.EventVenues?.Longitude ?? 0,
                    Date = eventEntity.Date,
                    Time = eventEntity.Time,
                    OnStream = eventEntity.OnStream ?? false,
                    StreamService = eventEntity.Stream?.StreamService,
                    StreamURL = eventEntity.Stream?.StreamURL,
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
               SportSubcategoryID = events.SportSubcategoryID,
               EventVenuesID = events.EventVenuesID,
               Date = events.Date,
               Time = events.Time,
               OnStream = events.OnStream,
               StreamID = events.StreamID,
               IsFinished = events.IsFinished,
               Attachement = events.Attachement,
               Archived = events.Archived,
               Deleted = events.Deleted,
               UserID = events.UserID,
           };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventsDTO.Events>>> GetEvents(
        [FromQuery] string? ID = null,
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
                query = query.Where(x => x.StreamID == streamID.Value);

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

            existingEvent.SportSubcategoryID = events.SportSubcategoryID;
            existingEvent.EventVenuesID = events.EventVenuesID;
            existingEvent.Date = events.Date;
            existingEvent.Time = events.Time;
            existingEvent.OnStream = events.OnStream;
            existingEvent.StreamID = events.StreamID;
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
                SportSubcategoryID = events.SportSubcategoryID,
                EventVenuesID = events.EventVenuesID,
                Date = events.Date,
                Time = events.Time,
                OnStream = events.OnStream,
                StreamID = events.StreamID,
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
                query = query.Where(x => x.FacebookLink.Contains(facebookLink));

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




        // Event Streams
        private static EventsDTO.EventStreams EventStreamsDTOMapper(EventStreams eventStreams) =>
           new EventsDTO.EventStreams
           {
               ID = eventStreams.ID,
               StreamService = eventStreams.StreamService,
               StreamURL = eventStreams.StreamURL
           };

        [HttpGet("Streams")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventStreams>>> GetEventStreams(
        [FromQuery] int? ID = null,
        [FromQuery] string? streamService = null,
        [FromQuery] string? streamURL = null)
        {
            var query = _context.EventStreams.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(streamService))
                query = query.Where(x => x.StreamService.Contains(streamService));

            if (!string.IsNullOrEmpty(streamURL))
                query = query.Where(x => x.StreamURL.Contains(streamURL));

            return await query
                .Select(x => EventStreamsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Streams/{id}")]
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

            existingEventStream.StreamService = eventStreamsDto.StreamService;
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

        [HttpPost("Streams")]
        public async Task<ActionResult<EventStreams>> PostEventStreams(EventsDTO.EventStreams eventStreams)
        {
            var eventStreamsDTO = new EventStreams
            {
                ID = eventStreams.ID,
                StreamService = eventStreams.StreamService,
                StreamURL = eventStreams.StreamURL
            };

            _context.EventStreams.Add(eventStreamsDTO);
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

            return CreatedAtAction("GetEventStreams", new { id = eventStreams.ID }, EventStreamsDTOMapper(eventStreamsDTO));
        }

        [HttpDelete("Streams/{id}")]
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
                query = query.Where(x => x.Address.Contains(address));

            if (!string.IsNullOrEmpty(venue))
                query = query.Where(x => x.Venue.Contains(venue));

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




        // Event Versus

        private static EventsDTO.EventVersus EventVersusDTOMapper(EventVersus eventVersus) =>
           new EventsDTO.EventVersus
           {
               ID = eventVersus.ID,
               Score = eventVersus.Score,
               SchoolRegionID = eventVersus.SchoolRegionID,
               EventID = eventVersus.EventID,
               RecentUpdateAt = eventVersus.RecentUpdateAt,
           };

        [HttpGet("Versus")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersus>>> GetEventVersus(
        [FromQuery] int? ID = null,
        [FromQuery] int? score = null,
        [FromQuery] int? schoolRegionID = null,
        [FromQuery] string? eventID = null,
        [FromQuery] DateTime? recentUpdateAt = null)
        {
            var query = _context.EventVersus.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (score.HasValue)
                query = query.Where(x => x.Score == score.Value);

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            if (!string.IsNullOrEmpty(eventID))
                query = query.Where(x => x.EventID == eventID);

            if (recentUpdateAt.HasValue)
                query = query.Where(x => x.RecentUpdateAt == recentUpdateAt.Value);

            return await query
                .Select(x => EventVersusDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Versus/{id}")]
        public async Task<IActionResult> PutEventVersus(int id, EventsDTO.EventVersus eventVersus)
        {
            if (id != eventVersus.ID)
            {
                return BadRequest();
            }

            var existingEventVersus = await _context.EventVersus.FindAsync(id);
            if (existingEventVersus == null)
            {
                return NotFound();
            }

            existingEventVersus.Score = eventVersus.Score;
            existingEventVersus.SchoolRegionID = eventVersus.SchoolRegionID;
            existingEventVersus.EventID = eventVersus.EventID;
            existingEventVersus.RecentUpdateAt = eventVersus.RecentUpdateAt;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVersusExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("Versus")]
        public async Task<ActionResult<EventVersus>> PostEventVersus(EventsDTO.EventVersus eventVersus)
        {
            var eventVersusDTO = new EventVersus
            {
                ID = eventVersus.ID,
                Score = eventVersus.Score,
                SchoolRegionID = eventVersus.SchoolRegionID,
                EventID = eventVersus.EventID,
                RecentUpdateAt = eventVersus.RecentUpdateAt,
            };

            _context.EventVersus.Add(eventVersusDTO);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEventVersus", new { id = eventVersus.ID }, EventVersusDTOMapper(eventVersusDTO));
        }

        [HttpDelete("Versus/{id}")]
        public async Task<IActionResult> DeleteEventVersus(int id)
        {
            var eventVersus = await _context.EventVersus.FindAsync(id);
            if (eventVersus == null)
            {
                return NotFound();
            }

            _context.EventVersus.Remove(eventVersus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventVersusExists(int id)
        {
            return _context.EventVersus.Any(e => e.ID == id);
        }
    }
}
