using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BilletingQuartersController : ControllerBase
    {
        private readonly palaro_2026Context _context; // Replace with your actual DbContext class

        public BilletingQuartersController(palaro_2026Context context)
        {
            _context = context;
        }

        [HttpGet("BilletingQuartersPerRegion")]
        public async Task<ActionResult<IEnumerable<BilletingQuartersDTO.bq_RegionBilletingQuartersDTO>>> GetBilletingQuartersPerRegion()
        {
            // Fetching data from BilletingQuarters and joining with RegionalTeams
            var billetingQuartersData = await (from b in _context.BilletingQuarters
                                               join r in _context.RegionalTeams on b.RegionalTeamID equals r.ID into regionalGroup
                                               from regional in regionalGroup.DefaultIfEmpty()
                                               select new BilletingQuartersDTO.bq_RegionBilletingQuartersDTO
                                               {
                                                   RegionalTeamName = regional != null ? regional.RegionalTeamName : null,
                                                   RegionalTeamNameAbbreviation = regional != null ? regional.RegionalTeamNameAbbreviation : null,
                                                   SchoolName = b.SchoolName,
                                                   SchoolAddress = b.SchoolAddress,
                                                   Latitude = b.Latitude,
                                                   Longitude = b.Longitude,
                                                   ContactPerson = b.ContactPerson,
                                                   ContactPersonNumber = b.ContactPersonNumber
                                               }).ToListAsync();

            return Ok(billetingQuartersData); // Returning the data as a response
        }


        // GET: api/BilletingQuarters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BilletingQuartersDTO.bq_BilletingQuartersDTO>>> GetBilletingQuarters()
        {
            var billetingQuarters = await _context.BilletingQuarters.AsNoTracking().ToListAsync();

            return Ok(billetingQuarters);
        }


        // GET: api/BilletingQuarters/5
        [HttpGet("{ID}")]
        public async Task<ActionResult<BilletingQuartersDTO.bq_BilletingQuartersDTO>> GetBilletingQuarters(int ID)
        {
            var billetingQuarter = await _context.BilletingQuarters.FindAsync(ID);

            if (billetingQuarter == null)
            {
                return NotFound();
            }

            return Ok(billetingQuarter);
        }

        // POST: api/BilletingQuarters
        [HttpPost]
        public async Task<ActionResult<BilletingQuartersDTO.bq_BilletingQuartersDTO>> CreateBilletingQuarter(
            [FromForm] int? RegionalTeamID,
            [FromForm] string? SchoolName,
            [FromForm] string? SchoolAddress,
            [FromForm] decimal? Latitude,
            [FromForm] decimal? Longitude,
            [FromForm] string? ContactPerson,
            [FromForm] string? ContactPersonNumber)
        {
            var billetingQuarter = new BilletingQuarters // Correctly referenced variable name
            {
                RegionalTeamID = RegionalTeamID,
                SchoolName = SchoolName,
                SchoolAddress = SchoolAddress,
                Latitude = Latitude,
                Longitude = Longitude,
                ContactPerson = ContactPerson,
                ContactPersonNumber = ContactPersonNumber
            };

            _context.BilletingQuarters.Add(billetingQuarter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBilletingQuarters), new { ID = billetingQuarter.ID }, billetingQuarter); // Corrected variable name here
        }

        // PUT: api/BilletingQuarters/5
        [HttpPut("{ID}")]
        public async Task<IActionResult> UpdateBilletingQuarter(int ID,
        [FromForm] int? RegionalTeamID,
        [FromForm] string? SchoolName,
        [FromForm] string? SchoolAddress,
        [FromForm] decimal? lat,
        [FromForm] decimal? _long,
        [FromForm] string? ContactPerson,
        [FromForm] string? ContactPersonNumber)
        {
            // Retrieve the existing billeting quarter by ID
            var billetingQuarter = await _context.BilletingQuarters.FindAsync(ID);

            if (billetingQuarter == null)
            {
                return NotFound(); // Return 404 if the billeting quarter is not found
            }

            // Update the properties of the existing entity
            billetingQuarter.RegionalTeamID = RegionalTeamID;
            billetingQuarter.SchoolName = SchoolName;
            billetingQuarter.SchoolAddress = SchoolAddress;
            billetingQuarter.Latitude = lat;
            billetingQuarter.Longitude = _long;
            billetingQuarter.ContactPerson = ContactPerson;
            billetingQuarter.ContactPersonNumber = ContactPersonNumber;

            // No need to set the ID since it is already the same as the existing entity

            try
            {
                // Save the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency exceptions
                if (!_context.BilletingQuarters.Any(b => b.ID == ID))
                {
                    return NotFound(); // If the entity was deleted by another process
                }
                else
                {
                    throw; // Rethrow if another error occurred
                }
            }

            return NoContent(); // Return 204 No Content for a successful update
        }



        // DELETE: api/BilletingQuarters/5
        [HttpDelete("{ID}")]
        public async Task<IActionResult> DeleteBilletingQuarter(int ID)
        {
            var billetingQuarter = await _context.BilletingQuarters.FindAsync(ID);

            if (billetingQuarter == null)
            {
                return NotFound();
            }

            _context.BilletingQuarters.Remove(billetingQuarter);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
