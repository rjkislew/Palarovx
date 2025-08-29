using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;
using Server.Palaro2026.DTO;
namespace Server.Palaro2026.Controller
{
    public partial class SchoolsController : ControllerBase
    {
        // Mapping Schools entity to SchoolsDTO.Schools
        private static SchoolsDTO.Schools SchoolsDTOMapper(Schools schools) =>
           new SchoolsDTO.Schools
           {
               ID = schools.ID,
               School = schools.School,
               SchoolDivisionID = schools.SchoolDivisionID,
               SchoolLevelsID = schools.SchoolLevelsID,
           };

        [HttpGet] // /api/Schools
        public async Task<ActionResult<IEnumerable<SchoolsDTO.Schools>>> GetSchools(
        [FromQuery] int? id = null,
        [FromQuery] string? school = null,
        [FromQuery] int? schoolDivisionID = null,
        [FromQuery] int? schoolLevelsID = null)
        {
            var query = _context.Schools.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(school))
                query = query.Where(x => x.School!.Contains(school));

            if (schoolDivisionID.HasValue)
                query = query.Where(x => x.SchoolDivisionID == schoolDivisionID.Value);

            if (schoolLevelsID.HasValue)
                query = query.Where(x => x.SchoolLevelsID == schoolLevelsID.Value);

            return await query
                .Select(x => SchoolsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost] // /api/Schools
        public async Task<ActionResult<Schools>> PostSchools(SchoolsDTO.Schools schools)
        {
            var schoolsDTO = new Schools
            {
                ID = schools.ID,
                School = schools.School,
                SchoolDivisionID = schools.SchoolDivisionID,
                SchoolLevelsID = schools.SchoolLevelsID,
            };

            _context.Schools.Add(schoolsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SchoolsExist(schools.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSchools", new { id = schools.ID }, SchoolsDTOMapper(schoolsDTO));
        }

        [HttpPut("{id}")] // /api/Schools/{id}
        public async Task<IActionResult> PutSchools(int id, SchoolsDTO.Schools schoolsDto)
        {
            if (schoolsDto == null || id != schoolsDto.ID)
            {
                return BadRequest("Invalid school ID or request body.");
            }

            // Fetch the existing entity from the database
            var existingSchool = await _context.Schools.FindAsync(id);
            if (existingSchool == null)
            {
                return NotFound();
            }

            // Map DTO properties to the entity
            existingSchool.School = schoolsDto.School;
            existingSchool.SchoolDivisionID = schoolsDto.SchoolDivisionID;
            existingSchool.SchoolLevelsID = schoolsDto.SchoolLevelsID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolsExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("{id}")] // /api/Schools/{id}
        public async Task<IActionResult> PatchSchools(int id, [FromBody] SchoolsDTO.Schools updatedSchool)
        {
            var existingSchool = await _context.Schools.FindAsync(id);

            if (existingSchool == null) return NotFound();

            if (updatedSchool.School != null) existingSchool.School = updatedSchool.School;
            if (updatedSchool.SchoolDivisionID != null) existingSchool.SchoolDivisionID = updatedSchool.SchoolDivisionID;
            if (updatedSchool.SchoolLevelsID != null) existingSchool.SchoolLevelsID = updatedSchool.SchoolLevelsID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolsExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (SchoolsExist(updatedSchool.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")] // /api/Schools/{id}
        public async Task<IActionResult> DeleteSchools(int id)
        {
            var schools = await _context.Schools.FindAsync(id);
            if (schools == null)
            {
                return NotFound();
            }

            _context.Schools.Remove(schools);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a school exists by ID
        private bool SchoolsExist(int id)
        {
            return _context.Schools.Any(e => e.ID == id);
        }
    }
}
