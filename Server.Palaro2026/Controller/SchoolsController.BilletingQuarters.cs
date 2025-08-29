using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;
using Server.Palaro2026.DTO;
namespace Server.Palaro2026.Controller
{
    public partial class SchoolsController : ControllerBase
    {
        // Mapping SchoolBilletingQuarters entity to SchoolsDTO.SchoolBilletingQuarters
        private static SchoolsDTO.SchoolBilletingQuarters SchoolBilletingQuartersDTOMapper(SchoolBilletingQuarters schoolBilletingQuarters) =>
           new SchoolsDTO.SchoolBilletingQuarters
           {
               ID = schoolBilletingQuarters.ID,
               SchoolRegionID = schoolBilletingQuarters.SchoolRegionID,
               BilletingQuarter = schoolBilletingQuarters.BilletingQuarter,
               Address = schoolBilletingQuarters.Address,
               Latitude = schoolBilletingQuarters.Latitude,
               Longitude = schoolBilletingQuarters.Longitude,
               ContactPerson = schoolBilletingQuarters.ContactPerson,
               ContactPersonNumber = schoolBilletingQuarters.ContactPersonNumber
           };

        [HttpGet("BilletingQuarters")] // /api/Schools/BilletingQuarters
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolBilletingQuarters>>> GetSchoolBilletingQuarters(
        [FromQuery] int? ID = null,
        [FromQuery] int? schoolRegionID = null,
        [FromQuery] string? billetingQuarter = null,
        [FromQuery] string? address = null,
        [FromQuery] decimal? latitude = null,
        [FromQuery] decimal? longitude = null,
        [FromQuery] string? contactPerson = null,
        [FromQuery] string? contactPersonNumber = null)
        {
            var query = _context.SchoolBilletingQuarters.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            if (!string.IsNullOrEmpty(billetingQuarter))
                query = query.Where(x => x.BilletingQuarter!.Contains(billetingQuarter));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(x => x.Address!.Contains(address));

            if (latitude.HasValue)
                query = query.Where(x => x.Latitude == latitude.Value);

            if (longitude.HasValue)
                query = query.Where(x => x.Longitude == longitude.Value);

            if (!string.IsNullOrEmpty(contactPerson))
                query = query.Where(x => x.ContactPerson!.Contains(contactPerson));

            if (!string.IsNullOrEmpty(contactPersonNumber))
                query = query.Where(x => x.ContactPersonNumber!.Contains(contactPersonNumber));

            return await query
                .Select(x => SchoolBilletingQuartersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("BilletingQuarters")] // /api/Schools/BilletingQuarters
        public async Task<ActionResult<SchoolBilletingQuarters>> PostSchoolBilletingQuarters(SchoolsDTO.SchoolBilletingQuarters schoolBilletingQuarters)
        {
            var schoolBilletingQuartersDTO = new SchoolBilletingQuarters
            {
                ID = schoolBilletingQuarters.ID,
                SchoolRegionID = schoolBilletingQuarters.SchoolRegionID,
                BilletingQuarter = schoolBilletingQuarters.BilletingQuarter,
                Address = schoolBilletingQuarters.Address,
                Latitude = schoolBilletingQuarters.Latitude,
                Longitude = schoolBilletingQuarters.Longitude,
                ContactPerson = schoolBilletingQuarters.ContactPerson,
                ContactPersonNumber = schoolBilletingQuarters.ContactPersonNumber
            };
            _context.SchoolBilletingQuarters.Add(schoolBilletingQuartersDTO);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSchoolBilletingQuarters", new { id = schoolBilletingQuarters.ID }, SchoolBilletingQuartersDTOMapper(schoolBilletingQuartersDTO));
        }

        [HttpPut("BilletingQuarters/{id}")] // /api/Schools/BilletingQuarters/{id}
        public async Task<IActionResult> PutSchoolBilletingQuarters(int id, SchoolsDTO.SchoolBilletingQuarters schoolBilletingQuarters)
        {
            if (id != schoolBilletingQuarters.ID)
            {
                return BadRequest();
            }

            var existingBilletingQuarter = await _context.SchoolBilletingQuarters.FindAsync(id);
            if (existingBilletingQuarter == null)
            {
                return NotFound();
            }

            existingBilletingQuarter.SchoolRegionID = schoolBilletingQuarters.SchoolRegionID;
            existingBilletingQuarter.BilletingQuarter = schoolBilletingQuarters.BilletingQuarter;
            existingBilletingQuarter.Address = schoolBilletingQuarters.Address;
            existingBilletingQuarter.Latitude = schoolBilletingQuarters.Latitude;
            existingBilletingQuarter.Longitude = schoolBilletingQuarters.Longitude;
            existingBilletingQuarter.ContactPerson = schoolBilletingQuarters.ContactPerson;
            existingBilletingQuarter.ContactPersonNumber = schoolBilletingQuarters.ContactPersonNumber;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolBilletingQuartersExists(id))
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

        [HttpPatch("BilletingQuarters/{id}")] // /api/Schools/BilletingQuarters/{id}
        public async Task<IActionResult> PatchSchoolBilletingQuarters(int id, [FromBody] SchoolsDTO.SchoolBilletingQuarters updatedQuarter)
        {
            var existingQuarter = await _context.SchoolBilletingQuarters.FindAsync(id);
            if (existingQuarter == null) return NotFound();

            if (updatedQuarter.SchoolRegionID != null) existingQuarter.SchoolRegionID = updatedQuarter.SchoolRegionID;
            if (updatedQuarter.BilletingQuarter != null) existingQuarter.BilletingQuarter = updatedQuarter.BilletingQuarter;
            if (updatedQuarter.Address != null) existingQuarter.Address = updatedQuarter.Address;
            if (updatedQuarter.Latitude != null) existingQuarter.Latitude = updatedQuarter.Latitude;
            if (updatedQuarter.Longitude != null) existingQuarter.Longitude = updatedQuarter.Longitude;
            if (updatedQuarter.ContactPerson != null) existingQuarter.ContactPerson = updatedQuarter.ContactPerson;
            if (updatedQuarter.ContactPersonNumber != null) existingQuarter.ContactPersonNumber = updatedQuarter.ContactPersonNumber;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SchoolBilletingQuarters.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("BilletingQuarters/{id}")] // /api/Schools/BilletingQuarters/{id}
        public async Task<IActionResult> DeleteSchoolBilletingQuarters(int id)
        {
            var schoolBilletingQuarters = await _context.SchoolBilletingQuarters.FindAsync(id);
            if (schoolBilletingQuarters == null)
            {
                return NotFound();
            }

            _context.SchoolBilletingQuarters.Remove(schoolBilletingQuarters);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a School Billeting Quarter exists by ID
        private bool SchoolBilletingQuartersExists(int id)
        {
            return _context.SchoolBilletingQuarters.Any(e => e.ID == id);
        }
    }
}
