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
        [FromQuery] DateTime? date = null,
        [FromQuery] bool? onStream = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] string? userID = null)
        {
            try
            {
                // Start with the base query
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
                    .AsQueryable();

                // Apply filters based on query parameters
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

                if (date.HasValue)
                {
                    query = query.Where(e => e.Date == date.Value.Date);
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

                // Execute the query
                var eventEntities = await query.ToListAsync();

                if (eventEntities == null || !eventEntities.Any())
                {
                    return NotFound("No events found matching the criteria.");
                }

                // Map events to DTO
                var eventDTO = eventEntities
                    .Select(eventEntity => new EventsDTO.EventDetails.Event
                    {
                        ID = eventEntity.ID,

                        // Include EventVersus if it exists, otherwise return an empty list
                        EventVersusList = eventEntity.EventVersus
                            ?.GroupBy(ev => new { ev.Score, ev.SchoolRegion?.Region, ev.SchoolRegion?.Abbreviation, ev.RecentUpdateAt })
                            .Select(ev => new EventsDTO.EventDetails.EventVersus
                            {
                                Score = ev.Key.Score,
                                Region = ev.Key.Region,
                                Abbreviation = ev.Key.Abbreviation,
                                RecentUpdateAt = ev.Key.RecentUpdateAt,
                            }).ToList() ?? new List<EventsDTO.EventDetails.EventVersus>(), // Default to empty list if no EventVersus

                        Category = eventEntity.SportSubcategory?.Sport?.SportCategory?.Category,
                        Sport = eventEntity.SportSubcategory?.Sport?.Sport,
                        Subcategory = eventEntity.SportSubcategory?.Subcategory,
                        Gender = eventEntity.SportSubcategory?.SportGenderCategory?.Gender,
                        Level = eventEntity.SportSubcategory?.SchoolLevel?.Level,
                        Venue = eventEntity.EventVenues?.Venue,
                        Latitude = eventEntity.EventVenues?.Latitude ?? 0, // Default value for nullable double
                        Longitude = eventEntity.EventVenues?.Longitude ?? 0, // Default value for nullable double
                        Date = eventEntity.Date,
                        Time = eventEntity.Time,
                        OnStream = eventEntity.OnStream ?? false, // Default value for nullable bool
                        StreamService = eventEntity.Stream?.StreamService,
                        StreamURL = eventEntity.Stream?.StreamURL,
                        IsFinished = eventEntity.IsFinished,
                        Attachement = eventEntity.Attachement,
                        Archived = eventEntity.Archived,
                        Deleted = eventEntity.Deleted,
                        UserID = eventEntity.UserID,
                    }).ToList();

                return Ok(eventDTO);
            }
            catch (Exception ex)
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
        public async Task<ActionResult<IEnumerable<EventsDTO.Events>>> GetEvents()
        {
            return await _context.Events
                .Select(x => EventsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventsDTO.Events>> GetEvents(string id)
        {
            var events = await _context.Events.FindAsync(id);

            if (events == null)
            {
                return NotFound();
            }

            return EventsDTOMapper(events);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvents(string id, EventsDTO.Events events)
        {
            if (id != events.ID)
            {
                return BadRequest();
            }

            _context.Entry(events).State = EntityState.Modified;

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
        public async Task<ActionResult<IEnumerable<EventsDTO.EventNews>>> GetEventNews()
        {
            return await _context.EventNews
                .Select(x => EventNewsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("News/{id}")]
        public async Task<ActionResult<EventsDTO.EventNews>> GetEventNews(int id)
        {
            var eventNews = await _context.EventNews.FindAsync(id);

            if (eventNews == null)
            {
                return NotFound();
            }

            return EventNewsDTOMapper(eventNews);
        }

        [HttpPut("News/{id}")]
        public async Task<IActionResult> PutEventNews(int id, EventsDTO.EventNews eventNews)
        {
            if (id != eventNews.ID)
            {
                return BadRequest();
            }

            _context.Entry(eventNews).State = EntityState.Modified;

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
        public async Task<ActionResult<IEnumerable<EventsDTO.EventStreams>>> GetEventStreams()
        {
            return await _context.EventStreams
                .Select(x => EventStreamsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Streams/{id}")]
        public async Task<ActionResult<EventsDTO.EventStreams>> GetEventStreams(int id)
        {
            var eventStreams = await _context.EventStreams.FindAsync(id);

            if (eventStreams == null)
            {
                return NotFound();
            }

            return EventStreamsDTOMapper(eventStreams);
        }

        [HttpPut("Streams/{id}")]
        public async Task<IActionResult> PutEventStreams(int id, EventsDTO.EventStreams eventStreams)
        {
            if (id != eventStreams.ID)
            {
                return BadRequest();
            }

            _context.Entry(eventStreams).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
               Venue = eventVenues.Venue,
               Latitude = eventVenues.Latitude,
               Longitude = eventVenues.Longitude,
           };

        [HttpGet("Venues")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVenues>>> GetEventVenues()
        {
            return await _context.EventVenues
                .Select(x => EventVenuesDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Venues/{id}")]
        public async Task<ActionResult<EventsDTO.EventVenues>> GetEventVenues(int id)
        {
            var eventVenues = await _context.EventVenues.FindAsync(id);

            if (eventVenues == null)
            {
                return NotFound();
            }

            return EventVenuesDTOMapper(eventVenues);
        }

        [HttpPut("Venues/{id}")]
        public async Task<IActionResult> PutEventVenues(int id, EventsDTO.EventVenues eventVenues)
        {
            if (id != eventVenues.ID)
            {
                return BadRequest();
            }

            _context.Entry(eventVenues).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventVenuesExists(id))
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
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersus>>> GetEventVersus()
        {
            return await _context.EventVersus
                .Select(x => EventVersusDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Versus/{id}")]
        public async Task<ActionResult<EventsDTO.EventVersus>> GetEventVersus(int id)
        {
            var eventVersus = await _context.EventVersus.FindAsync(id);

            if (eventVersus == null)
            {
                return NotFound();
            }

            return EventVersusDTOMapper(eventVersus);
        }

        [HttpPut("Versus/{id}")]
        public async Task<IActionResult> PutEventVersus(int id, EventsDTO.EventVersus eventVersus)
        {
            if (id != eventVersus.ID)
            {
                return BadRequest();
            }

            _context.Entry(eventVersus).State = EntityState.Modified;

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
                else
                {
                    throw;
                }
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
