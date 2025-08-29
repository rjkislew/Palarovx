using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class EventsController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public EventsController(Palaro2026Context context)
        {
            _context = context;
        }

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
                    GamePhase = eventEntity.GamePhase,
                    Gender = eventEntity.SportSubcategory?.SportGenderCategory?.Gender,
                    Level = eventEntity.SportSubcategory?.SchoolLevel?.Level,
                    Venue = eventEntity.EventVenues?.Venue,
                    Address = eventEntity.EventVenues?.Address,
                    Latitude = eventEntity.EventVenues?.Latitude ?? 0,
                    Longitude = eventEntity.EventVenues?.Longitude ?? 0,
                    Date = eventEntity.Date,
                    Time = eventEntity.Time,
                    OnStream = eventEntity.OnStream ?? false,
                    StreamService = eventEntity.EventStream?.EventStreamService?.StreamService,
                    StreamTitle = eventEntity.EventStream?.StreamTitle,
                    StreamURL = eventEntity.EventStream?.StreamURL,
                    IsFinished = eventEntity.IsFinished,
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
    }
}
