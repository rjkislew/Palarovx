using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CounterController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public CounterController(Palaro2026Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<EventsDTO.EventCount>> GetEventCount()
        {
            try
            {
                var eventList = await _context.Events
                    .Include(e => e.EventVersusTeams)
                    .AsNoTracking()
                    .ToListAsync();

                var newsList = await _context.News
                    .AsNoTracking()
                    .ToListAsync();

                var eventStreamServicesList = await _context.EventStreamServices
                    .AsNoTracking()
                    .ToListAsync();

                var eventStreamsList = await _context.EventStreams
                    .AsNoTracking()
                    .ToListAsync();

                var eventVenuesList = await _context.EventVenues
                    .AsNoTracking()
                    .ToListAsync();

                var billetingQuartersList = await _context.SchoolBilletingQuarters
                    .AsNoTracking()
                    .ToListAsync();

                var coachesList = await _context.ProfileCoaches
                    .AsNoTracking()
                    .ToListAsync();

                var playersList = await _context.ProfilePlayers
                    .AsNoTracking()
                    .ToListAsync();

                var regionsList = await _context.SchoolRegions
                    .AsNoTracking()
                    .ToListAsync();

                var schoolsList = await _context.Schools
                    .Include(e => e.SchoolDivision)
                    .AsNoTracking()
                    .ToListAsync();

                var result = new EventsDTO.EventCount
                {
                    NumberOfEvents = eventList.Count(),
                    NumberOfFinishedEvents = eventList.Where(e => e.IsFinished == true).Count(),
                    NumberofOngoingEvents = eventList.Where(e => e.IsFinished == false).Count(),
                    NumberOfEventsThatLacksPlayers = eventList.Count(e => e.IsFinished == false && e.EventVersusTeams?.Count() < 2),
                    NumberOfNewsPublished = newsList.Count(),
                    NumberOfEventStreamServices = eventStreamServicesList.Count(),
                    NumberOfEventStreams = eventStreamsList.Count(),
                    NumberOfVenues = eventVenuesList.Count(),
                    NumberOfBilletingQuarters = billetingQuartersList.Count(),
                    NumberOfRegionalTeamsParticipating = regionsList.Count(),
                    NumberOfSchoolDivisionsParticipating = schoolsList.Where(e => e.SchoolDivision != null).Select(e => e.SchoolDivision?.ID).Distinct().Count(),
                    NumberOfSchoolsParticipating = schoolsList.Count(),
                    NumberOfCoaches = coachesList.Count(),
                    NumberOfPlayers = playersList.Count(),
                    NumberOfMalePlayers = playersList.Where(e => e.Sex == "M").Count(),
                    NumberOfFemalePlayers = playersList.Where(e => e.Sex == "F").Count(),
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }
    }
}
