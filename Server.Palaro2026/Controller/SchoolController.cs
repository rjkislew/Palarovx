
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;
using Server.Palaro2026.DTO;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolsController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public SchoolsController(Palaro2026Context context)
        {
            _context = context;
        }

        // Schools

        private static SchoolDTO.Schools SchoolsDTOMapper(Schools schools) =>
           new SchoolDTO.Schools
           {
               ID = schools.ID,
               School = schools.School,
               SchoolDivisionID = schools.SchoolDivisionID,
               SchoolLevelsID = schools.SchoolLevelsID,
           };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchoolDTO.Schools>>> GetSchools()
        {
            return await _context.Schools
                .Select(x => SchoolsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SchoolDTO.Schools>> GetSchools(int id)
        {
            var schools = await _context.Schools.FindAsync(id);

            if (schools == null)
            {
                return NotFound();
            }

            return SchoolsDTOMapper(schools);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchools(int id, SchoolDTO.Schools schools)
        {
            if (id != schools.ID)
            {
                return BadRequest();
            }

            _context.Entry(schools).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolsExists(id))
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

        [HttpPost]
        public async Task<ActionResult<Schools>> PostSchools(SchoolDTO.Schools schools)
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
                if (SchoolsExists(schools.ID))
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

        [HttpDelete("{id}")]
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

        private bool SchoolsExists(int id)
        {
            return _context.Schools.Any(e => e.ID == id);
        }




        // School Billeting Quarters

        private static SchoolDTO.SchoolBilletingQuarters SchoolBilletingQuartersDTOMapper(SchoolBilletingQuarters schoolBilletingQuarters) =>
           new SchoolDTO.SchoolBilletingQuarters
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

        [HttpGet("BilletingQuarters")]
        public async Task<ActionResult<IEnumerable<SchoolDTO.SchoolBilletingQuarters>>> GetSchoolBilletingQuarters()
        {
            return await _context.SchoolBilletingQuarters
                .Select(x => SchoolBilletingQuartersDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("BilletingQuarters/{id}")]
        public async Task<ActionResult<SchoolDTO.SchoolBilletingQuarters>> GetSchoolBilletingQuarters(int id)
        {
            var schoolBilletingQuarters = await _context.SchoolBilletingQuarters.FindAsync(id);

            if (schoolBilletingQuarters == null)
            {
                return NotFound();
            }

            return SchoolBilletingQuartersDTOMapper(schoolBilletingQuarters);
        }

        [HttpPut("BilletingQuarters/{id}")]
        public async Task<IActionResult> PutSchoolBilletingQuarters(int id, SchoolDTO.SchoolBilletingQuarters schoolBilletingQuarters)
        {
            if (id != schoolBilletingQuarters.ID)
            {
                return BadRequest();
            }

            _context.Entry(schoolBilletingQuarters).State = EntityState.Modified;

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

        [HttpPost("BilletingQuarters")]
        public async Task<ActionResult<SchoolBilletingQuarters>> PostSchoolBilletingQuarters(SchoolDTO.SchoolBilletingQuarters schoolBilletingQuarters)
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

        [HttpDelete("BilletingQuarters/{id}")]
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

        private bool SchoolBilletingQuartersExists(int id)
        {
            return _context.SchoolBilletingQuarters.Any(e => e.ID == id);
        }





        // School Division

        private static SchoolDTO.SchoolDivisions SchoolDivisionsDTOMapper(SchoolDivisions schoolDivisions) =>
           new SchoolDTO.SchoolDivisions
           {
               ID = schoolDivisions.ID,
               Division = schoolDivisions.Division,
               SchoolRegionID = schoolDivisions.SchoolRegionID
           };

        [HttpGet("Divisions")]
        public async Task<ActionResult<IEnumerable<SchoolDTO.SchoolDivisions>>> GetSchoolDivisions()
        {
            return await _context.SchoolDivisions
                .Select(x => SchoolDivisionsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Divisions/{id}")]
        public async Task<ActionResult<SchoolDTO.SchoolDivisions>> GetSchoolDivisions(int id)
        {
            var schoolDivisions = await _context.SchoolDivisions.FindAsync(id);

            if (schoolDivisions == null)
            {
                return NotFound();
            }

            return SchoolDivisionsDTOMapper(schoolDivisions);
        }

        [HttpPut("Divisions/{id}")]
        public async Task<IActionResult> PutSchoolDivisions(int id, SchoolDTO.SchoolDivisions schoolDivisions)
        {
            if (id != schoolDivisions.ID)
            {
                return BadRequest();
            }

            _context.Entry(schoolDivisions).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolDivisionsExists(id))
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

        [HttpPost("Divisions")]
        public async Task<ActionResult<SchoolDivisions>> PostSchoolDivisions(SchoolDTO.SchoolDivisions schoolDivisions)
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
                if (SchoolDivisionsExists(schoolDivisions.ID))
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

        [HttpDelete("Divisions/{id}")]
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

        private bool SchoolDivisionsExists(int id)
        {
            return _context.SchoolDivisions.Any(e => e.ID == id);
        }




        // School Level

        private static SchoolDTO.SchoolLevels SchoolLevelsDTOMapper(SchoolLevels schoolLevels) =>
           new SchoolDTO.SchoolLevels
           {
               ID = schoolLevels.ID,
               Level = schoolLevels.Level
           };

        [HttpGet("Levels")]
        public async Task<ActionResult<IEnumerable<SchoolDTO.SchoolLevels>>> GetSchoolLevels()
        {
            return await _context.SchoolLevels
                .Select(x => SchoolLevelsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Levels/{id}")]
        public async Task<ActionResult<SchoolDTO.SchoolLevels>> GetSchoolLevels(int id)
        {
            var schoolLevels = await _context.SchoolLevels.FindAsync(id);

            if (schoolLevels == null)
            {
                return NotFound();
            }

            return SchoolLevelsDTOMapper(schoolLevels);
        }

        [HttpPut("Levels/{id}")]
        public async Task<IActionResult> PutSchoolLevels(int id, SchoolDTO.SchoolLevels schoolLevels)
        {
            if (id != schoolLevels.ID)
            {
                return BadRequest();
            }

            _context.Entry(schoolLevels).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolLevelsExists(id))
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

        [HttpPost("Levels")]
        public async Task<ActionResult<SchoolLevels>> PostSchoolLevels(SchoolDTO.SchoolLevels schoolLevels)
        {
            var schoolLevelsDTO  = new SchoolLevels
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
                if (SchoolLevelsExists(schoolLevels.ID))
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

        [HttpDelete("Levels/{id}")]
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

        private bool SchoolLevelsExists(int id)
        {
            return _context.SchoolLevels.Any(e => e.ID == id);
        }




        // School Regions

        private static SchoolDTO.SchoolRegions SchoolRegionsDTOMapper(SchoolRegions schoolRegions) =>
           new SchoolDTO.SchoolRegions
           {
               ID = schoolRegions.ID,
               Region = schoolRegions.Region,
               Abbreviation = schoolRegions.Abbreviation
           };

        [HttpGet("Regions")]
        public async Task<ActionResult<IEnumerable<SchoolDTO.SchoolRegions>>> GetSchoolRegions()
        {
            return await _context.SchoolRegions
                .Select(x => SchoolRegionsDTOMapper(x))
                .ToListAsync();
        }

        [HttpGet("Regions/{id}")]
        public async Task<ActionResult<SchoolDTO.SchoolRegions>> GetSchoolRegions(int id)
        {
            var schoolRegions = await _context.SchoolRegions.FindAsync(id);

            if (schoolRegions == null)
            {
                return NotFound();
            }

            return SchoolRegionsDTOMapper(schoolRegions);
        }

        [HttpPut("Regions/{id}")]
        public async Task<IActionResult> PutSchoolRegions(int id, SchoolDTO.SchoolRegions schoolRegions)
        {
            if (id != schoolRegions.ID)
            {
                return BadRequest();
            }

            _context.Entry(schoolRegions).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolRegionsExists(id))
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

        [HttpPost("Regions")]
        public async Task<ActionResult<SchoolRegions>> PostSchoolRegions(SchoolDTO.SchoolRegions schoolRegions)
        {
            var schoolRegionsDTO = new SchoolRegions
            {
                ID = schoolRegions.ID,
                Region = schoolRegions.Region,
                Abbreviation = schoolRegions.Abbreviation
            };

            _context.SchoolRegions.Add(schoolRegionsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SchoolRegionsExists(schoolRegions.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSchoolRegions", new { id = schoolRegions.ID }, SchoolRegionsDTOMapper(schoolRegionsDTO));
        }

        [HttpDelete("Regions/{id}")]
        public async Task<IActionResult> DeleteSchoolRegions(int id)
        {
            var schoolRegions = await _context.SchoolRegions.FindAsync(id);
            if (schoolRegions == null)
            {
                return NotFound();
            }

            _context.SchoolRegions.Remove(schoolRegions);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SchoolRegionsExists(int id)
        {
            return _context.SchoolRegions.Any(e => e.ID == id);
        }
    }
}
