using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        [HttpGet("Venues")]
        public async Task<ActionResult<IEnumerable<VenuesDTO.v_VenuesDTO>>> GetVenuesView()
        {
            var venuesData = await _context.Venues
                .Select(v => new VenuesDTO.v_VenuesDTO
                {
                    ID = v.ID,
                    Venue = v.Venue,
                    Latitude = v.Latitude,
                    Longitude = v.Longitude
                }).ToListAsync();

            return Ok(venuesData);
        }

        [HttpGet("GetVenues")]
        public async Task<ActionResult<IEnumerable<Venues>>> GetVenues()
        {
            var venuesList = await _context.Venues.AsNoTracking().ToListAsync();
            return Ok(venuesList);
        }

        [HttpGet("GetVenue/{ID}")]
        public async Task<ActionResult<Venues>> GetVenue(int ID)
        {
            var venue = await _context.Venues.FindAsync(ID);

            if (venue == null)
            {
                return NotFound();
            }

            return Ok(venue);
        }

        [HttpPost("CreateVenue")]
        public async Task<ActionResult<Venues>> CreateVenue(Venues venue)
        {
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVenue), new { venue.ID }, venue);
        }

        [HttpPut("UpdateVenue/{ID}")]
        public async Task<IActionResult> UpdateVenue(int ID, Venues venue)
        {
            if (ID != venue.ID)
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
                if (!_context.Venues.Any(v => v.ID == ID))
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

        [HttpDelete("DeleteVenue/{ID}")]
        public async Task<IActionResult> DeleteVenue(int ID)
        {
            var venue = await _context.Venues.FindAsync(ID);

            if (venue == null)
            {
                return NotFound();
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
