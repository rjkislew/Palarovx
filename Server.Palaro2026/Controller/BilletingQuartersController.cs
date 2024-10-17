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
        public async Task<ActionResult<IEnumerable<BilletingQuartersDTO>>> GetBilletingQuartersPerRegion()
        {
            // Fetching data from billeting_quarters and joining with regional_teams
            var billetingQuartersData = await (from b in _context.billeting_quarters
                                               join r in _context.regional_teams on b.regional_team_id equals r.id into regionalGroup
                                               from regional in regionalGroup.DefaultIfEmpty()
                                               select new BilletingQuartersDTO.bq_BilletingQuartersDTO
                                               {
                                                   regional_team_name = regional != null ? regional.regional_team_name : null,
                                                   regional_team_name_abbreviation = regional != null ? regional.regional_team_name_abbreviation : null,
                                                   school_name = b.school_name,
                                                   school_address = b.school_address,
                                                   latitude = b.latitude,
                                                   longitude = b.longitude,
                                                   contact_person = b.contact_person,
                                                   contact_person_number = b.contact_person_number
                                               }).ToListAsync();

            return Ok(billetingQuartersData); // Returning the data as a response
        }


        // GET: api/BilletingQuarters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<billeting_quarters>>> GetBilletingQuarters()
        {
            return await _context.billeting_quarters.AsNoTracking().ToListAsync();
        }

        // GET: api/BilletingQuarters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<billeting_quarters>> GetBilletingQuarters(int id)
        {
            var billetingQuarter = await _context.billeting_quarters.FindAsync(id);

            if (billetingQuarter == null)
            {
                return NotFound();
            }

            return Ok(billetingQuarter);
        }

        // POST: api/BilletingQuarters
        [HttpPost]
        public async Task<ActionResult<billeting_quarters>> CreateBilletingQuarter(
            [FromForm] int? regional_team_id,
            [FromForm] string? school_name,
            [FromForm] string? school_address,
            [FromForm] decimal? latitude,
            [FromForm] decimal? longitude,
            [FromForm] string? contact_person,
            [FromForm] string? contact_person_number)
        {
            var billetingQuarter = new billeting_quarters // Correctly referenced variable name
            {
                regional_team_id = regional_team_id,
                school_name = school_name,
                school_address = school_address,
                latitude = latitude,
                longitude = longitude,
                contact_person = contact_person,
                contact_person_number = contact_person_number
            };

            _context.billeting_quarters.Add(billetingQuarter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBilletingQuarters), new { id = billetingQuarter.id }, billetingQuarter); // Corrected variable name here
        }

        // PUT: api/BilletingQuarters/5
        // PUT: api/BilletingQuarters/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBilletingQuarter(int id,
    [FromForm] int? regional_team_id,
    [FromForm] string? school_name,
    [FromForm] string? school_address,
    [FromForm] decimal? lat,
    [FromForm] decimal? _long,
    [FromForm] string? contact_person,
    [FromForm] string? contact_person_number)
        {
            // Retrieve the existing billeting quarter by ID
            var billetingQuarter = await _context.billeting_quarters.FindAsync(id);

            if (billetingQuarter == null)
            {
                return NotFound(); // Return 404 if the billeting quarter is not found
            }

            // Update the properties of the existing entity
            billetingQuarter.regional_team_id = regional_team_id;
            billetingQuarter.school_name = school_name;
            billetingQuarter.school_address = school_address;
            billetingQuarter.latitude = lat;
            billetingQuarter.longitude = _long;
            billetingQuarter.contact_person = contact_person;
            billetingQuarter.contact_person_number = contact_person_number;

            // No need to set the ID since it is already the same as the existing entity

            try
            {
                // Save the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency exceptions
                if (!_context.billeting_quarters.Any(b => b.id == id))
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilletingQuarter(int id)
        {
            var billetingQuarter = await _context.billeting_quarters.FindAsync(id);

            if (billetingQuarter == null)
            {
                return NotFound();
            }

            _context.billeting_quarters.Remove(billetingQuarter);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
