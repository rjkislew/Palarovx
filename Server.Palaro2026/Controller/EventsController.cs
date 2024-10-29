using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;


        /// 
        /// 
        /// VIEWS
        /// 
        /// 

        // Read
        [HttpGet("EventDetails")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventDetail.ED_SportCategoriesContent>>> GetEventDetails()
        {
            try
            {
                // Fetch the data from the database
                var sports = await _context.EventDetails
                    .AsNoTracking()
                    .ToListAsync();

                // Group the sports by category
                var groupedSports = sports
                    .GroupBy(c => c.Category)
                    .Select(category => new EventsDTO.EventDetail.ED_SportCategoriesContent
                    {
                        Category = category.Key,
                        SportList = category
                        .GroupBy(s => new { s.Sport })
                        .Select(sport => new EventsDTO.EventDetail.ED_SportsContent
                        {
                            Sport = sport.Key.Sport,
                            LevelList = sport
                            .GroupBy(l => l.Level)
                            .Select(level => new EventsDTO.EventDetail.ED_SchoolLevelsContent
                            {
                                Level = level.Key,
                                GenderList = level
                                .GroupBy(gc => gc.Gender)
                                .Select(gender => new EventsDTO.EventDetail.ED_GenderCategoriesContent
                                {
                                    Gender = gender.Key,
                                    SportSubCategoryList = gender
                                    .GroupBy(sc => sc.SubCategory)
                                    .Select(subCategory => new EventsDTO.EventDetail.ED_SubCategoriesContent
                                    {
                                        SubCategory = subCategory.Key,
                                        VenueList = subCategory
                                        .GroupBy(v => v.Venue)
                                        .Select(venue => new EventsDTO.EventDetail.ED_VenuesContent
                                        {
                                            Venue = venue.Key,
                                            EventList = venue
                                            .GroupBy(e => new { e.EventTitle, e.Date, e.Time, e.OnStream, e.StreamURL, e.IsFinished, e.Attachement, e.Archived, e.Deleted })
                                            .Select(events => new EventsDTO.EventDetail.ED_EventsContent
                                            {
                                                EventTitle = events.Key.EventTitle,
                                                Date = events.Key.Date,
                                                Time = events.Key.Time,
                                                OnStream = events.Key.OnStream,
                                                StreamURL = events.Key.StreamURL,
                                                IsFinished = events.Key.IsFinished,
                                                Attachement = events.Key.Attachement,
                                                Archived = events.Key.Archived,
                                                Deleted = events.Key.Deleted,
                                                TeamList = events
                                                .Select(teams => new EventsDTO.EventDetail.ED_RegionsContent
                                                {
                                                    Region = teams.Region,
                                                    Abbreviation = teams.Abbreviation,
                                                    Score = teams.Score
                                                }).ToList()
                                            }).ToList()
                                        }).ToList()
                                    }).ToList()
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList();

                return Ok(groupedSports);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }


        /// 
        /// 
        /// EVENT
        /// 
        /// 

        // Create
        [HttpPost("Event")]
        public async Task<ActionResult<EventsDTO.Events.EventsContent>> CreateEvent([FromBody] EventsDTO.Events.EventsContent eventContent)
        {
            try
            {
                var events = new Entities.Events
                {
                    ID = eventContent.ID,
                    SportSubCategoryID = eventContent.SportSubCategoryID,
                    VenueID = eventContent.VenueID,
                    EventTitle = eventContent.EventTitle,
                    Date = eventContent.Date,
                    Time = eventContent.Time,
                    OnStream = eventContent.OnStream,
                    StreamURL = eventContent.StreamURL,
                    IsFinished = eventContent.IsFinished,
                    Archived = eventContent.Archived,
                    Deleted = eventContent.Deleted
                };

                _context.Events.Add(events);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEvents), new { id = events.ID }, eventContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("Event")]
        public async Task<ActionResult<IEnumerable<EventsDTO.Events.EventsContent>>> GetEvents()
        {
            try
            {
                var events = await _context.Events.AsNoTracking().ToListAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("Event/{id}")]
        public async Task<IActionResult> UpdateEvent(int id, EventsDTO.Events.EventsContent eventContent)
        {
            if (id != eventContent.ID)
            {
                return BadRequest("Event ID mismatch");
            }

            try
            {
                var events = new Entities.Events
                {
                    ID = eventContent.ID,
                    SportSubCategoryID = eventContent.SportSubCategoryID,
                    VenueID = eventContent.VenueID,
                    EventTitle = eventContent.EventTitle,
                    Date = eventContent.Date,
                    Time = eventContent.Time,
                    OnStream = eventContent.OnStream,
                    StreamURL = eventContent.StreamURL,
                    IsFinished = eventContent.IsFinished,
                    Archived = eventContent.Archived,
                    Deleted = eventContent.Deleted
                };

                _context.Events.Attach(events);
                _context.Entry(events).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Events.Any(e => e.ID == id))
                {
                    return NotFound($"Event with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("Event/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var events = await _context.Events.FindAsync(id);
            if (events == null)
            {
                return NotFound($"Event with ID {id} not found");
            }

            _context.Events.Remove(events);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// 
        /// 
        /// EVENT VERSUS
        /// 
        /// 

        // Create
        [HttpPost("EventVersus")]
        public async Task<ActionResult<EventsDTO.EventVersus.EventVersusContent>> CreateEventVersus([FromBody] EventsDTO.EventVersus.EventVersusContent eventVersusContent)
        {
            try
            {
                var eventsVersus = new Entities.EventVersus
                {
                    ID = eventVersusContent.ID,
                    Score = eventVersusContent.Score,
                    RegionID = eventVersusContent.RegionID,
                    EventID = eventVersusContent.EventID,
                };

                _context.EventVersus.Add(eventsVersus);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEventVersus), new { id = eventsVersus.ID }, eventVersusContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("EventVersus")]
        public async Task<ActionResult<IEnumerable<EventsDTO.EventVersus.EventVersusContent>>> GetEventVersus()
        {
            try
            {
                var eventVersus = await _context.EventVersus.AsNoTracking().ToListAsync();
                return Ok(eventVersus);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("EventVersus/{id}")]
        public async Task<IActionResult> UpdateEventVersus(int id, EventsDTO.EventVersus.EventVersusContent eventVersusContent)
        {
            if (id != eventVersusContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var eventVersus = new Entities.EventVersus
                {
                    ID = eventVersusContent.ID,
                    Score = eventVersusContent.Score,
                    RegionID = eventVersusContent.RegionID,
                    EventID = eventVersusContent.EventID
                };

                _context.EventVersus.Attach(eventVersus);
                _context.Entry(eventVersus).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EventVersus.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("EventVersus/{id}")]
        public async Task<IActionResult> DeleteEventVersus(int id)
        {
            var eventVersus = await _context.EventVersus.FindAsync(id);
            if (eventVersus == null)
            {
                return NotFound($"Event Versus with ID {id} not found");
            }

            _context.EventVersus.Remove(eventVersus);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
