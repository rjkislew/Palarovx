using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;
using Server.Palaro2026.DTO;
namespace Server.Palaro2026.Controller
{
    public partial class SchoolsController : ControllerBase
    {
        // Mapping SchoolLevels entity to SchoolsDTO.SchoolLevels
        private static SchoolsDTO.SchoolLevels SchoolLevelsDTOMapper(SchoolLevels schoolLevels) =>
           new SchoolsDTO.SchoolLevels
           {
               ID = schoolLevels.ID,
               Level = schoolLevels.Level
           };

        [HttpGet("Levels")] // /api/Schools/Levels
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolLevels>>> GetSchoolLevels(
        [FromQuery] int? id = null,
        [FromQuery] string? level = null)
        {
            var query = _context.SchoolLevels.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(x => x.Level!.Contains(level));

            return await query
                .Select(x => SchoolLevelsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Levels")] // /api/Schools/Levels
        public async Task<ActionResult<SchoolLevels>> PostSchoolLevels(SchoolsDTO.SchoolLevels schoolLevels)
        {
            var schoolLevelsDTO = new SchoolLevels
            {
                ID = schoolLevels.ID,
                Level = schoolLevels.Level
            };
            _context.SchoolLevels.Add(schoolLevelsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SchoolLevelsExist(schoolLevels.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSchoolLevels", new { id = schoolLevels.ID }, SchoolLevelsDTOMapper(schoolLevelsDTO));
        }

        [HttpPut("Levels/{id}")] // /api/Schools/Levels/{id}
        public async Task<IActionResult> PutSchoolLevels(int id, SchoolsDTO.SchoolLevels schoolLevels)
        {
            if (id != schoolLevels.ID)
            {
                return BadRequest();
            }

            var existingLevel = await _context.SchoolLevels.FindAsync(id);
            if (existingLevel == null)
            {
                return NotFound();
            }

            existingLevel.Level = schoolLevels.Level;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolLevelsExist(id))
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

        [HttpPatch("Levels/{id}")] // /api/Schools/Levels/{id}
        public async Task<IActionResult> PatchSchoolLevels(int id, [FromBody] SchoolsDTO.SchoolLevels updatedLevel)
        {
            var existingLevel = await _context.SchoolLevels.FindAsync(id);
            if (existingLevel == null) return NotFound();

            if (updatedLevel.Level != null) existingLevel.Level = updatedLevel.Level;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SchoolLevels.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Levels/{id}")] // /api/Schools/Levels/{id}
        public async Task<IActionResult> DeleteSchoolLevels(int id)
        {
            var schoolLevels = await _context.SchoolLevels.FindAsync(id);
            if (schoolLevels == null)
            {
                return NotFound();
            }

            _context.SchoolLevels.Remove(schoolLevels);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a School Level exists by ID
        private bool SchoolLevelsExist(int id)
        {
            return _context.SchoolLevels.Any(e => e.ID == id);
        }
    }
}
