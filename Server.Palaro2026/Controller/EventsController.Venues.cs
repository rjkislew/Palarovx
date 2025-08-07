using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;


namespace Server.Palaro2026.Controller
{
    public partial class EventsController : ControllerBase
    {// Event Venues REST methods

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
    }
}
