
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

        [HttpGet("Details")]
        public async Task<ActionResult<List<SchoolsDTO.SchoolDetails.Schools>>> GetSchoolDetails()
        {
            try
            {
                var schools = await _context.Schools
                    .Include(s => s.SchoolDivision)
                        .ThenInclude(d => d!.SchoolRegion)
                    .Include(s => s.SchoolLevels)
                    .AsNoTracking()
                    .ToListAsync();

                var SchoolsDTO = schools
                    .Where(s => s.SchoolDivision?.SchoolRegion != null) // Ensure valid data
                    .Select(s => new SchoolsDTO.SchoolDetails.Schools
                    {
                        ID = s.ID,
                        School = s.School,
                        Level = s.SchoolLevels?.Level,
                        Division = s.SchoolDivision?.Division,
                        Region = s.SchoolDivision?.SchoolRegion?.Region,
                        Abbreviation = s.SchoolDivision?.SchoolRegion?.Abbreviation
                    })
                    .ToList();

                return Ok(SchoolsDTO);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }


        // Schools

        private static SchoolsDTO.Schools SchoolsDTOMapper(Schools schools) =>
           new SchoolsDTO.Schools
           {
               ID = schools.ID,
               School = schools.School,
               SchoolDivisionID = schools.SchoolDivisionID,
               SchoolLevelsID = schools.SchoolLevelsID,
           };

        [HttpGet]
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
                query = query.Where(x => x.School.Contains(school));

            if (schoolDivisionID.HasValue)
                query = query.Where(x => x.SchoolDivisionID == schoolDivisionID.Value);

            if (schoolLevelsID.HasValue)
                query = query.Where(x => x.SchoolLevelsID == schoolLevelsID.Value);

            return await query
                .Select(x => SchoolsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("{id}")]
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
                if (!SchoolsExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost]
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





        [HttpGet("BilletingQuarters/Details")]
        public async Task<ActionResult<List<SchoolsDTO.SchoolBillingQuarterDetails.SchoolBilletingQuarters>>> GetBilletingQuartersDetails()
        {
            try
            {
                var billetingQuarters = await _context.SchoolBilletingQuarters
                    .Include(s => s.SchoolRegion)
                    .ToListAsync();

                var billetingQuarterDTO = billetingQuarters
                    .Where(quarter => quarter.SchoolRegion != null)
                    .Select(quarter => new SchoolsDTO.SchoolBillingQuarterDetails.SchoolBilletingQuarters
                    {
                        ID = quarter.ID,
                        Region = quarter.SchoolRegion?.Region,
                        Abbreviation = quarter.SchoolRegion?.Abbreviation,
                        BilletingQuarter = quarter.BilletingQuarter,
                        Address = quarter.Address,
                        Latitude = quarter.Latitude,
                        Longitude = quarter.Longitude,
                        ContactPerson = quarter.ContactPerson,
                        ContactPersonNumber = quarter.ContactPersonNumber                        
                    })
                    .ToList();

                return Ok(billetingQuarterDTO);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }
        // School Billeting Quarters

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

        [HttpGet("BilletingQuarters")]
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
                query = query.Where(x => x.BilletingQuarter.Contains(billetingQuarter));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(x => x.Address.Contains(address));

            if (latitude.HasValue)
                query = query.Where(x => x.Latitude == latitude.Value);

            if (longitude.HasValue)
                query = query.Where(x => x.Longitude == longitude.Value);

            if (!string.IsNullOrEmpty(contactPerson))
                query = query.Where(x => x.ContactPerson.Contains(contactPerson));

            if (!string.IsNullOrEmpty(contactPersonNumber))
                query = query.Where(x => x.ContactPersonNumber.Contains(contactPersonNumber));

            return await query
                .Select(x => SchoolBilletingQuartersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("BilletingQuarters/{id}")]
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

        [HttpPost("BilletingQuarters")]
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

        private static SchoolsDTO.SchoolDivisions SchoolDivisionsDTOMapper(SchoolDivisions schoolDivisions) =>
           new SchoolsDTO.SchoolDivisions
           {
               ID = schoolDivisions.ID,
               Division = schoolDivisions.Division,
               SchoolRegionID = schoolDivisions.SchoolRegionID
           };

        [HttpGet("Divisions")]
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolDivisions>>> GetSchoolDivisions(
        [FromQuery] int? id = null,
        [FromQuery] string? division = null,
        [FromQuery] int? schoolRegionID = null)
        {
            var query = _context.SchoolDivisions.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(division))
                query = query.Where(x => x.Division.Contains(division));

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);

            return await query
                .Select(x => SchoolDivisionsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Divisions/{id}")]
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
        private static SchoolsDTO.SchoolLevels SchoolLevelsDTOMapper(SchoolLevels schoolLevels) =>
           new SchoolsDTO.SchoolLevels
           {
               ID = schoolLevels.ID,
               Level = schoolLevels.Level
           };

        [HttpGet("Levels")]
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolLevels>>> GetSchoolLevels(
        [FromQuery] int? id = null,
        [FromQuery] string? level = null)
        {
            var query = _context.SchoolLevels.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(x => x.Level.Contains(level));

            return await query
                .Select(x => SchoolLevelsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Levels/{id}")]
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

        private static SchoolsDTO.SchoolRegions SchoolRegionsDTOMapper(SchoolRegions schoolRegions) =>
           new SchoolsDTO.SchoolRegions
           {
               ID = schoolRegions.ID,
               Region = schoolRegions.Region,
               Abbreviation = schoolRegions.Abbreviation
           };

        [HttpGet("Regions")]
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolRegions>>> GetSchoolRegions(
        [FromQuery] int? id = null,
        [FromQuery] string? region = null,
        [FromQuery] string? abbreviation = null)
        {
            var query = _context.SchoolRegions.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(region))
                query = query.Where(x => x.Region.Contains(region));

            if (!string.IsNullOrEmpty(abbreviation))
                query = query.Where(x => x.Abbreviation.Contains(abbreviation));

            return await query
                .Select(x => SchoolRegionsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPut("Regions/{id}")]
        public async Task<IActionResult> PutSchoolRegions(int id, SchoolsDTO.SchoolRegions schoolRegions)
        {
            if (id != schoolRegions.ID)
            {
                return BadRequest();
            }

            var existingRegion = await _context.SchoolRegions.FindAsync(id);
            if (existingRegion == null)
            {
                return NotFound();
            }

            existingRegion.Region = schoolRegions.Region;
            existingRegion.Abbreviation = schoolRegions.Abbreviation;

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
        public async Task<ActionResult<SchoolRegions>> PostSchoolRegions(SchoolsDTO.SchoolRegions schoolRegions)
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
