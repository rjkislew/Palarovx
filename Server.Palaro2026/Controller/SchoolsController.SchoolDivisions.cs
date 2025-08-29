using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;
using Server.Palaro2026.DTO;
namespace Server.Palaro2026.Controller
{
    public partial class SchoolsController : ControllerBase
    {
        // Mapping SchoolDivisions entity to SchoolsDTO.SchoolDivisions
        private static SchoolsDTO.SchoolDivisions SchoolDivisionsDTOMapper(SchoolDivisions schoolDivisions) =>
           new SchoolsDTO.SchoolDivisions
           {
               ID = schoolDivisions.ID,
               Division = schoolDivisions.Division,
               SchoolRegionID = schoolDivisions.SchoolRegionID
           };

        [HttpGet("Divisions")] // /api/Schools/Divisions
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolDivisions>>> GetSchoolDivisions(
        [FromQuery] int? id = null,
        [FromQuery] string? division = null,
        [FromQuery] int? schoolRegionID = null)
        {
            var query = _context.SchoolDivisions.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(division))
                query = query.Where(x => x.Division!.Contains(division));

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            return await query
                .Select(x => SchoolDivisionsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Divisions")] // /api/Schools/Divisions
        public async Task<ActionResult<SchoolDivisions>> PostSchoolDivisions(SchoolsDTO.SchoolDivisions schoolDivisions)
        {
            var schoolDivisionsDTO = new SchoolDivisions
            {
                ID = schoolDivisions.ID,
                Division = schoolDivisions.Division,
                SchoolRegionID = schoolDivisions.SchoolRegionID
            };
            _context.SchoolDivisions.Add(schoolDivisionsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SchoolDivisionsExist(schoolDivisions.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSchoolDivisions", new { id = schoolDivisions.ID }, SchoolDivisionsDTOMapper(schoolDivisionsDTO));
        }

        [HttpPut("Divisions/{id}")] // /api/Schools/Divisions/{id}
        public async Task<IActionResult> PutSchoolDivisions(int id, SchoolsDTO.SchoolDivisions schoolDivisions)
        {
            if (id != schoolDivisions.ID)
            {
                return BadRequest();
            }

            var existingDivision = await _context.SchoolDivisions.FindAsync(id);
            if (existingDivision == null)
            {
                return NotFound();
            }

            existingDivision.Division = schoolDivisions.Division;
            existingDivision.SchoolRegionID = schoolDivisions.SchoolRegionID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolDivisionsExist(id))
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

        [HttpPatch("Divisions/{id}")] // /api/Schools/Divisions/{id}
        public async Task<IActionResult> PatchSchoolDivisions(int id, [FromBody] SchoolsDTO.SchoolDivisions updatedDivision)
        {
            var existingDivision = await _context.SchoolDivisions.FindAsync(id);
            if (existingDivision == null) return NotFound();

            if (updatedDivision.Division != null) existingDivision.Division = updatedDivision.Division;
            if (updatedDivision.SchoolRegionID != null) existingDivision.SchoolRegionID = updatedDivision.SchoolRegionID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SchoolDivisions.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Divisions/{id}")] // /api/Schools/Divisions/{id}
        public async Task<IActionResult> DeleteSchoolDivisions(int id)
        {
            var schoolDivisions = await _context.SchoolDivisions.FindAsync(id);
            if (schoolDivisions == null)
            {
                return NotFound();
            }

            _context.SchoolDivisions.Remove(schoolDivisions);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a School Division exists by ID
        private bool SchoolDivisionsExist(int id)
        {
            return _context.SchoolDivisions.Any(e => e.ID == id);
        }
    }
}
