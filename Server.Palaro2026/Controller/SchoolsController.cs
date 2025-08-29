using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;
using Server.Palaro2026.DTO;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class SchoolsController : ControllerBase
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

        // ------------------------------------------------------------------------------------------------------------------

        // School Divisions view

        [HttpGet("Divisions/Details")] // /api/Schools/Divisions/Details
        public async Task<ActionResult<IEnumerable<SchoolsDTO.SchoolDivisionsDetails>>> GetSchoolDivisionsDetails()
        {
            var query = _context.SchoolDivisions
                .Include(sd => sd.SchoolRegion)
                .AsQueryable();

            var result = await query
                .AsNoTracking()
                .Select(x => new SchoolsDTO.SchoolDivisionsDetails
                {
                    ID = x.ID,
                    Division = x.Division,
                    SchoolRegionID = x.SchoolRegionID,
                    Region = x.SchoolRegion != null ? x.SchoolRegion.Region : null,
                    Abbreviation = x.SchoolRegion != null ? x.SchoolRegion.Abbreviation : null
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
