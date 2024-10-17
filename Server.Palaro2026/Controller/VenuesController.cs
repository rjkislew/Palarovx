using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly palaro_2026Context _context;

        public VenuesController(palaro_2026Context context)
        {
            _context = context;
        }
        [HttpGet("Venues")]
        public async Task<ActionResult<IEnumerable<VenuesDTO>>> GetVenuesView()
        {
            var venuesData = await _context.venues
                .Select(v => new VenuesDTO.v_VenusDTO
                {
                    location = v.location,
                    latitude = v.latitude,
                    longitude = v.longitude
                }).ToListAsync();

            return Ok(venuesData);
        }

        [HttpGet("GetVenues")]
        public async Task<ActionResult<IEnumerable<venues>>> GetVenues()
        {
            var venuesList = await _context.venues.AsNoTracking().ToListAsync();
            return Ok(venuesList);
        }

        [HttpGet("GetVenue/{id}")]
        public async Task<ActionResult<venues>> GetVenue(int id)
        {
            var venue = await _context.venues.FindAsync(id);

            if (venue == null)
            {
                return NotFound();
            }

            return Ok(venue);
        }

        [HttpPost("CreateVenue")]
        public async Task<ActionResult<venues>> CreateVenue(venues venue)
        {
            _context.venues.Add(venue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVenue), new { id = venue.id }, venue);
        }

        [HttpPut("UpdateVenue/{id}")]
        public async Task<IActionResult> UpdateVenue(int id, venues venue)
        {
            if (id != venue.id)
            {
                return BadRequest();
            }

            _context.Entry(venue).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.venues.Any(v => v.id == id))
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

        [HttpDelete("DeleteVenue/{id}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venue = await _context.venues.FindAsync(id);

            if (venue == null)
            {
                return NotFound();
            }

            _context.venues.Remove(venue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
