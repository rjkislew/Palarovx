using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO.View;

namespace Server.Palaro2026.Controller.View
{
    [Route("api/[controller]")]
    [ApiController]
    public class vw_venuesController : ControllerBase
    {
        private readonly palaro_2026Context _context;

        public vw_venuesController(palaro_2026Context context)
        {
            _context = context;
        }

        [HttpGet("getVenueLocation")]
        public async Task<ActionResult<IEnumerable<vw_venuesDTO>>> GetVenueLocations()
        {
            var venues = await _context.vw_venues.ToListAsync();

            var groupedVenues = venues
                .Select(venue => new vw_venuesDTO
                {
                    location = venue.location,
                    latitude = venue.latitude,
                    longitude = venue.longitude,
                }).ToList();

            return groupedVenues;
        }
    }
}
