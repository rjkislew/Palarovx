using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionalTeamsController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        [HttpGet("SchoolDetails")]
        public async Task<ActionResult<IEnumerable<RegionalTeamsDTO.SchoolDetails.SD_DivisionsContent>>> GetSportCategoriesDetails()
        {
            try
            {
                // Fetch the data from the database
                var schools = await _context.SchoolDetails
                    .AsNoTracking()
                    .ToListAsync();

                // Group the sports by category
                var groupedSchools = schools
                    .GroupBy(r => new { r.Region, r.Abbreviation })
                    .Select(region => new RegionalTeamsDTO.SchoolDetails.SD_RegionsContent
                    {
                        Region = region.Key.Region,
                        Abbreviation = region.Key.Abbreviation,
                        DivisionList = region
                        .GroupBy(d => d.Division)
                        .Select(division => new RegionalTeamsDTO.SchoolDetails.SD_DivisionsContent
                        {
                            Division = division.Key,
                            SchoolList = division
                            .Select(school => new RegionalTeamsDTO.SchoolDetails.SD_SchoolsContent
                            {
                                School = school.School
                            }).ToList()
                        }).ToList()
                    }).ToList();

                return Ok(groupedSchools);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }

        /// 
        /// 
        /// REGION
        /// 
        /// 

        // Create
        [HttpPost("Region")]
        public async Task<ActionResult<RegionalTeamsDTO.RegionsDetail.RegionsContent>> CreateRegion(RegionalTeamsDTO.RegionsDetail.RegionsContent regionContent)
        {
            try
            {
                var region = new Regions
                {
                    ID = regionContent.ID,
                    Region = regionContent.Region,
                    Abbreviation = regionContent.Abbreviation,
                };

                _context.Regions.Add(region);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRegion), new { id = region.ID }, regionContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("Region")]
        public async Task<ActionResult<IEnumerable<RegionalTeamsDTO.RegionsDetail.RegionsContent>>> GetRegion()
        {
            try
            {
                var regions = await _context.Regions.AsNoTracking().ToListAsync();
                return Ok(regions);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("Region/{id}")]
        public async Task<IActionResult> UpdateRegions(int id, RegionalTeamsDTO.RegionsDetail.RegionsContent regionContent)
        {
            if (id != regionContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {

                var region = new Regions
                {
                    ID = regionContent.ID,
                    Region = regionContent.Region,
                    Abbreviation = regionContent.Abbreviation,
                };

                _context.Regions.Attach(region);
                _context.Entry(region).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Regions.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("Region/{id}")]
        public async Task<IActionResult> DeleteSportCategory(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null)
            {
                return NotFound($"Region with ID {id} not found");
            }

            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// 
        /// 
        /// DIVISION
        /// 
        /// 

        // Create
        [HttpPost("Division")]
        public async Task<ActionResult<RegionalTeamsDTO.DivisionsDetail.DivisionsContent>> CreateDivision(RegionalTeamsDTO.DivisionsDetail.DivisionsContent divisionContent)
        {

            try
            {
                var division = new Divisions
                {
                    ID = divisionContent.ID,
                    Division = divisionContent.Division
                };

                _context.Divisions.Add(division);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDivision), new { id = division.ID }, divisionContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("Division")]
        public async Task<ActionResult<IEnumerable<RegionalTeamsDTO.DivisionsDetail.DivisionsContent>>> GetDivision()
        {
            try
            {
                var divisions = await _context.Divisions.AsNoTracking().ToListAsync();
                return Ok(divisions);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("Division/{id}")]
        public async Task<IActionResult> UpdateDivisions(int id, RegionalTeamsDTO.DivisionsDetail.DivisionsContent divisionContent)
        {
            if (id != divisionContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var division = new Divisions
                {
                    ID = divisionContent.ID,
                    Division = divisionContent.Division
                };

                _context.Divisions.Attach(division);
                _context.Entry(division).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Divisions.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("Division/{id}")]
        public async Task<IActionResult> DeleteDivision(int id)
        {
            var division = await _context.Divisions.FindAsync(id);
            if (division == null)
            {
                return NotFound($"Division with ID {id} not found");
            }

            _context.Divisions.Remove(division);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// 
        /// 
        /// SCHOOL
        /// 
        /// 

        // Create
        [HttpPost("School")]
        public async Task<ActionResult<RegionalTeamsDTO.SchoolsDetail.SchoolsContent>> CreateSchool(RegionalTeamsDTO.SchoolsDetail.SchoolsContent schoolContent)
        {
            try
            {
                var school = new Schools
                {
                    ID = schoolContent.ID,
                    School = schoolContent.School
                };

                _context.Schools.Add(school);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSchool), new { id = school.ID }, schoolContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("School")]
        public async Task<ActionResult<IEnumerable<RegionalTeamsDTO.SchoolsDetail.SchoolsContent>>> GetSchool()
        {
            try
            {
                var schools = await _context.Schools.AsNoTracking().ToListAsync();
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("School/{id}")]
        public async Task<IActionResult> UpdateSchools(int id, RegionalTeamsDTO.SchoolsDetail.SchoolsContent schoolContent)
        {
            if (id != schoolContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var school = new Schools
                {
                    ID = schoolContent.ID,
                    School = schoolContent.School
                };

                _context.Schools.Attach(school);
                _context.Entry(school).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Schools.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("School/{id}")]
        public async Task<IActionResult> DeleteSchool(int id)
        {
            var school = await _context.Schools.FindAsync(id);
            if (school == null)
            {
                return NotFound($"Sport Category with ID {id} not found");
            }

            _context.Schools.Remove(school);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
