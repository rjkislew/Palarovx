using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        [HttpGet("EventDetails")]
        public async Task<ActionResult<IEnumerable<EventsDTO.e_EventsDTO>>> GetEventsWithTeams()
        {
            var eventsData = await (from e in _context.Events
                                    join rtA in _context.RegionalTeams on e.RegionalTeamAID equals rtA.ID into teamAGroup
                                    from teamA in teamAGroup.DefaultIfEmpty()
                                    join rtB in _context.RegionalTeams on e.RegionalTeamBID equals rtB.ID into teamBGroup
                                    from teamB in teamBGroup.DefaultIfEmpty()
                                    join winner in _context.RegionalTeams on e.WinnerID equals winner.ID into winnerGroup
                                    from w in winnerGroup.DefaultIfEmpty()
                                    join loser in _context.RegionalTeams on e.LoserID equals loser.ID into loserGroup
                                    from l in loserGroup.DefaultIfEmpty()
                                    join v in _context.Venues on e.venueID equals v.ID into venueGroup
                                    from venue in venueGroup.DefaultIfEmpty()
                                    join subCat in _context.SportSubCategories on e.SportSubCategoryID equals subCat.ID into subCategoryGroup
                                    from subCategory in subCategoryGroup.DefaultIfEmpty() // Join to get the subcategory
                                    select new EventsDTO.e_EventsDTO
                                    {
                                        ID = e.ID,
                                        TeamA = teamA != null ? teamA.RegionalTeamName : null,
                                        TeamAAbbreviation = teamA != null ? teamA.RegionalTeamNameAbbreviation : null,
                                        TeamAFinalScore = e.TeamAFinalScore,
                                        TeamB = teamB != null ? teamB.RegionalTeamName : null,
                                        TeamBAbbreviation = teamB != null ? teamB.RegionalTeamNameAbbreviation : null,
                                        TeamBFinalScore = e.TeamBFinalScore,
                                        SportSubCategory = subCategory != null ? subCategory.SubCategory : null, // Assuming SubCategoryName is the property
                                        Venue = venue != null ? venue.Venue : null,
                                        EventTitle = e.EventTitle,
                                        Date = e.Date,
                                        Time = e.Time,
                                        OnStream = e.OnStream,
                                        StreamURL = e.StreamURL,
                                        IsFinished = e.IsFinished,
                                        Archived = e.Archived,
                                        Deleted = e.Deleted,
                                        Attachement = e.Attachement,
                                        LoserTeam = l != null ? l.RegionalTeamName : null,
                                        LoserTeamAbbreviation = l != null ? l.RegionalTeamNameAbbreviation : null,
                                        WinnerTeam = w != null ? w.RegionalTeamName : null,
                                        WinnerTeamAbbreviation = w != null ? w.RegionalTeamNameAbbreviation : null,
                                    }).ToListAsync();

            return Ok(eventsData); // Returning the data as a response
        }



        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Events>>> GetEvents()
        {
            var eventsData = await (from e in _context.Events
                                        // Include any necessary joins here
                                    select new Events
                                    {
                                        ID = e.ID,
                                        RegionalTeamAID = e.RegionalTeamAID,
                                        TeamAFinalScore = e.TeamAFinalScore,
                                        RegionalTeamBID = e.RegionalTeamBID,
                                        TeamBFinalScore = e.TeamBFinalScore,
                                        SportSubCategoryID = e.SportSubCategoryID,
                                        venueID = e.venueID,
                                        EventTitle = e.EventTitle,
                                        Date = e.Date,
                                        Time = e.Time,
                                        OnStream = e.OnStream,
                                        StreamURL = e.StreamURL,
                                        IsFinished = e.IsFinished,
                                        Archived = e.Archived,
                                        Deleted = e.Deleted,
                                        Attachement = e.Attachement,
                                        LoserID = e.LoserID,
                                        WinnerID = e.WinnerID
                                    }).ToListAsync();

            return Ok(eventsData);
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Events>> GetEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var eventData = new Events
            {
                ID = eventEntity.ID,
                RegionalTeamAID = eventEntity.RegionalTeamAID,
                TeamAFinalScore = eventEntity.TeamAFinalScore,
                RegionalTeamBID = eventEntity.RegionalTeamBID,
                TeamBFinalScore = eventEntity.TeamBFinalScore,
                SportSubCategoryID = eventEntity.SportSubCategoryID,
                venueID = eventEntity.venueID,
                EventTitle = eventEntity.EventTitle,
                Date = eventEntity.Date,
                Time = eventEntity.Time,
                OnStream = eventEntity.OnStream,
                StreamURL = eventEntity.StreamURL,
                IsFinished = eventEntity.IsFinished,
                Archived = eventEntity.Archived,
                Deleted = eventEntity.Deleted,
                Attachement = eventEntity.Attachement,
                LoserID = eventEntity.LoserID,
                WinnerID = eventEntity.WinnerID
            };

            return Ok(eventData);
        }

        // POST: api/events
        [HttpPost("AddEvent")]
        public async Task<ActionResult<EventsDTO.e_EventsDTO>> CreateEvent([FromBody] EventsDTO.e_CreateEventDTO createEventDto)
        {
            var newEvent = new Events
            {
                RegionalTeamAID = createEventDto.RegionalTeamAID,
                TeamAFinalScore = createEventDto.TeamAFinalScore,
                RegionalTeamBID = createEventDto.RegionalTeamBID,
                TeamBFinalScore = createEventDto.TeamBFinalScore,
                SportSubCategoryID = createEventDto.SportSubCategoryID,
                venueID = createEventDto.venueID,
                EventTitle = createEventDto.EventTitle,
                Date = createEventDto.Date,
                Time = createEventDto.Time,
                OnStream = createEventDto.OnStream,
                StreamURL = createEventDto.StreamURL,

                // Auto-increment ID will be handled by the database
                Archived = false,  // Set Archived to false
                Deleted = false,   // Set Deleted to false
                IsFinished = false,  // Set IsFinished to false (0)
                Attachement = null,
            };

            // Add the new event to the context
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            // Retrieve the newly created event with related data
            var createdEvent = await (from e in _context.Events
                                      where e.ID == newEvent.ID
                                      join teamA in _context.RegionalTeams on e.RegionalTeamAID equals teamA.ID into teamAGroup
                                      from teamA in teamAGroup.DefaultIfEmpty()
                                      join teamB in _context.RegionalTeams on e.RegionalTeamBID equals teamB.ID into teamBGroup
                                      from teamB in teamBGroup.DefaultIfEmpty()
                                      join winner in _context.RegionalTeams on e.WinnerID equals winner.ID into winnerGroup
                                      from winner in winnerGroup.DefaultIfEmpty()
                                      join loser in _context.RegionalTeams on e.LoserID equals loser.ID into loserGroup
                                      from loser in loserGroup.DefaultIfEmpty()
                                      join venue in _context.Venues on e.venueID equals venue.ID into venueGroup
                                      from venue in venueGroup.DefaultIfEmpty()
                                      join subCat in _context.SportSubCategories on e.SportSubCategoryID equals subCat.ID into subCategoryGroup
                                      from subCategory in subCategoryGroup.DefaultIfEmpty() // Join to get the subcategory
                                      select new EventsDTO.e_EventsDTO
                                      {
                                          ID = e.ID,
                                          TeamA = teamA.RegionalTeamName,
                                          TeamAAbbreviation = teamA.RegionalTeamNameAbbreviation,
                                          TeamAFinalScore = e.TeamAFinalScore,
                                          TeamB = teamB.RegionalTeamName,
                                          TeamBAbbreviation = teamB.RegionalTeamNameAbbreviation,
                                          TeamBFinalScore = e.TeamBFinalScore,
                                          SportSubCategory = subCategory.SubCategory,
                                          Venue = venue.Venue,
                                          EventTitle = e.EventTitle,
                                          Date = e.Date,
                                          Time = e.Time,
                                          OnStream = e.OnStream,
                                          StreamURL = e.StreamURL,
                                          IsFinished = e.IsFinished,
                                          Archived = e.Archived,
                                          Deleted = e.Deleted,
                                          Attachement = e.Attachement,
                                          LoserTeam = loser.RegionalTeamName,
                                          LoserTeamAbbreviation = loser.RegionalTeamNameAbbreviation,
                                          WinnerTeam = winner.RegionalTeamName,
                                          WinnerTeamAbbreviation = winner.RegionalTeamNameAbbreviation
                                      }).FirstOrDefaultAsync();

            if (createdEvent == null)
            {
                return NotFound();
            }

            // Return the created event with all related data
            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.ID }, createdEvent);
        }



        // PUT: api/events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] Events updatedEvent)
        {
            if (id != updatedEvent.ID)
            {
                return BadRequest();
            }

            _context.Entry(updatedEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
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

        // DELETE: api/events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.ID == id);
        }


    }
}
