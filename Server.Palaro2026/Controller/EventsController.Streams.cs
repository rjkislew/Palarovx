using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;


namespace Server.Palaro2026.Controller
{
    public partial class EventsController : ControllerBase
    {
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

    }
}
