
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

        // ------------------------------------------------------------------------------------------------------------------

        // Schools details view

        [HttpGet("Details")] // /api/Schools/Details
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

        // Schools REST methods

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

        // ------------------------------------------------------------------------------------------------------------------

        // School Billeting Quarters view
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

        // School Billeting Quarters REST methods

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

        // ------------------------------------------------------------------------------------------------------------------

        // School Divisions REST methods

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

        // ------------------------------------------------------------------------------------------------------------------

        // School Level REST methods

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

        // ------------------------------------------------------------------------------------------------------------------

        // School Regions REST methods

        // Mapping SchoolRegions entity to SchoolsDTO.SchoolRegions
        private static SchoolsDTO.SchoolRegions SchoolRegionsDTOMapper(SchoolRegions schoolRegions) =>
           new SchoolsDTO.SchoolRegions
           {
               ID = schoolRegions.ID,
               Region = schoolRegions.Region,
               Abbreviation = schoolRegions.Abbreviation
           };

        [HttpGet("Regions")] // /api/Schools/Regions
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolRegions>>> GetSchoolRegions(
        [FromQuery] int? id = null,
        [FromQuery] string? region = null,
        [FromQuery] string? abbreviation = null)
        {
            var query = _context.SchoolRegions.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.ID == id.Value);

            if (!string.IsNullOrEmpty(region))
                query = query.Where(x => x.Region!.Contains(region));

            if (!string.IsNullOrEmpty(abbreviation))
                query = query.Where(x => x.Abbreviation!.Contains(abbreviation));

            return await query
                .Select(x => SchoolRegionsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Regions")] // /api/Schools/Regions
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
                if (SchoolRegionsExist(schoolRegions.ID))
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

        [HttpPut("Regions/RegionLogo/{region}")]
        public async Task<IActionResult> UploadRegionLogo(string region, [FromForm] IFormFile? logoFile)
        {
            if (logoFile == null || logoFile.Length == 0)
            {
                return BadRequest("No logo file uploaded or file is empty.");
            }

            try
            {
                // 1. Find region
                var regions = await _context.SchoolRegions
                    .FirstOrDefaultAsync(r => r.Region != null && r.Region.ToLower() == region.ToLower());

                if (regions == null)
                {
                    return NotFound($"Region with name '{region}' not found.");
                }

                // 2. Validate extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(logoFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid logo file type. Only .jpg, .jpeg, .png, and .webp are allowed.");
                }

                // 3. Validate file size
                if (logoFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Logo file size exceeds the 5 MB limit.");
                }

                // 4. Path to logos
                var basePath = @"\\192.168.2.210\pgas_attachment\palaro2026\media\region\region_logo";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                // 5. Sanitize region name
                var sanitizedRegionName = string.Concat(regions.Region!.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

                // 6. Delete any files like Caraga.*
                var wildcardPattern = $"{sanitizedRegionName}.*";
                var matchingFiles = Directory.GetFiles(basePath, wildcardPattern);
                foreach (var file in matchingFiles)
                {
                    System.IO.File.Delete(file);
                }

                // 7. Save new logo
                var newFileName = $"{sanitizedRegionName}{fileExtension}";
                var fullPath = Path.Combine(basePath, newFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                return Ok(new
                {
                    message = "Logo uploaded successfully.",
                    fileName = newFileName,
                    storagePath = fullPath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading logo: {ex.Message}");
            }
        }



        [HttpPut("Regions/TeamLogo/{region}")]
        public async Task<IActionResult> UploadRegionTeamLogo(string region, [FromForm] IFormFile? logoFile)
        {
            if (logoFile == null || logoFile.Length == 0)
            {
                return BadRequest("No logo file uploaded or file is empty.");
            }

            try
            {
                // 1. Find region
                var regions = await _context.SchoolRegions
                    .FirstOrDefaultAsync(r => r.Region != null && r.Region.ToLower() == region.ToLower());

                if (regions == null)
                {
                    return NotFound($"Region with name '{region}' not found.");
                }

                // 2. Validate extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(logoFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid logo file type. Only .jpg, .jpeg, .png, and .webp are allowed.");
                }

                // 3. Validate file size
                if (logoFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Logo file size exceeds the 5 MB limit.");
                }

                // 4. Path to logos
                var basePath = @"\\192.168.2.210\pgas_attachment\palaro2026\media\region\team_logo";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                // 5. Sanitize region name
                var sanitizedRegionName = string.Concat(regions.Region!.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

                // 6. Delete any files like Caraga.*
                var wildcardPattern = $"{sanitizedRegionName}.*";
                var matchingFiles = Directory.GetFiles(basePath, wildcardPattern);
                foreach (var file in matchingFiles)
                {
                    System.IO.File.Delete(file);
                }

                // 7. Save new logo
                var newFileName = $"{sanitizedRegionName}{fileExtension}";
                var fullPath = Path.Combine(basePath, newFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                return Ok(new
                {
                    message = "Logo uploaded successfully.",
                    fileName = newFileName,
                    storagePath = fullPath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading logo: {ex.Message}");
            }
        }



        [HttpPut("Regions/{id}")] // /api/Schools/Regions/{id}  
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
                if (!SchoolRegionsExist(id))
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

        [HttpPatch("Regions/{id}")] // /api/Schools/Regions/{id}
        public async Task<IActionResult> PatchSchoolRegions(int id, [FromBody] SchoolsDTO.SchoolRegions updatedRegion)
        {
            var existingRegion = await _context.SchoolRegions.FindAsync(id);
            if (existingRegion == null) return NotFound();

            if (updatedRegion.Region != null) existingRegion.Region = updatedRegion.Region;
            if (updatedRegion.Abbreviation != null) existingRegion.Abbreviation = updatedRegion.Abbreviation;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SchoolRegions.Any(e => e.ID == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }


        [HttpDelete("Regions/{id}")] // /api/Schools/Regions/{id}
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

        // Check if a School Region exists by ID
        private bool SchoolRegionsExist(int id)
        {
            return _context.SchoolRegions.Any(e => e.ID == id);
        }
    }
}
