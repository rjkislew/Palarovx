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


        /// 
        /// 
        /// VENUES
        /// 
        /// 

        // Create
        [HttpPost("Venue")]
        public async Task<ActionResult<VenuesDTO.Venues.VenuesContents>> CreateSport([FromBody] VenuesDTO.Venues.VenuesContents venuesContent)
        {
            try
            {
                var venues = new Venues
                {
                    ID = venuesContent.ID,
                    Venue = venuesContent.Venue,
                    Latitude = venuesContent.Latitude,
                    Longitude = venuesContent.Longitude,
                };

                _context.Venues.Add(venues);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVenues), new { id = venues.ID }, venuesContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("Venue")]
        public async Task<ActionResult<IEnumerable<VenuesDTO.Venues.VenuesContents>>> GetVenues()
        {
            try
            {
                var venues = await _context.Venues.AsNoTracking().ToListAsync();
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("Venue/{id}")]
        public async Task<IActionResult> UpdateVenue(int id, VenuesDTO.Venues.VenuesContents venuesContent)
        {
            if (id != venuesContent.ID)
            {
                return BadRequest("Venue ID mismatch");
            }

            try
            {
                var venues = new Venues
                {
                    ID = venuesContent.ID,
                    Venue = venuesContent.Venue,
                    Latitude = venuesContent.Latitude,
                    Longitude = venuesContent.Longitude,
                };

                _context.Venues.Attach(venues);
                _context.Entry(venues).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Venues.Any(e => e.ID == id))
                {
                    return NotFound($"Venue with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("Venue/{id}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound($"Venue with ID {id} not found");
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
