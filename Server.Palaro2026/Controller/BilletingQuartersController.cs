using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BilletingQuartersController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        /// 
        /// 
        /// VIEWS
        /// 
        /// 

        [HttpGet("BilletingQuartersDetails")]
        public async Task<ActionResult<IEnumerable<BilletingQuartersDTO.BilletingQuartersDetails.BQD_RegionContent>>> GetBilletingQuartersDetails()
        {
            try
            {
                // Fetch the data from the database
                var billetingQuarters = await _context.BilletingQuartersDetails
                    .AsNoTracking()
                    .ToListAsync();

                // Group the sports by category
                var groupedBilletingQuarters = billetingQuarters
                    .GroupBy(r => new { r.Region, r.Abbreviation })
                    .Select(region => new BilletingQuartersDTO.BilletingQuartersDetails.BQD_RegionContent
                    {
                        Region = region.Key.Region,
                        Abbreviation = region.Key.Abbreviation,
                        BilletingQuarterList = region
                        .Select(billetingQuarters => new BilletingQuartersDTO.BilletingQuartersDetails.BQD_BilletingQuartersContent
                        {
                            BilletingQuarter = billetingQuarters.BilletingQuarter,
                            Address = billetingQuarters.Address,
                            Latitude = billetingQuarters.Latitude,
                            Longitude = billetingQuarters.Longitude,
                            ContactPerson = billetingQuarters.ContactPerson,
                            ContactPersonNumber = billetingQuarters.ContactPersonNumber
                        }).ToList()
                    }).ToList();

                return Ok(groupedBilletingQuarters);
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
        /// BILLETING QUARTERS
        /// 
        /// 

        // Create
        [HttpPost("BilletingQuarter")]
        public async Task<ActionResult<BilletingQuartersDTO.BilletingQuarters.BilletingQuartersContent>> CreateBilletingQuartert([FromBody] BilletingQuartersDTO.BilletingQuarters.BilletingQuartersContent billetingQuartersContent)
        {
            try
            {
                var billetingQuarters = new BilletingQuarters
                {
                    ID = billetingQuartersContent.ID,
                    RegionID = billetingQuartersContent.RegionID,
                    Address = billetingQuartersContent.Address,
                    Latitude = billetingQuartersContent.Latitude,
                    Longitude = billetingQuartersContent.Longitude,
                    ContactPerson = billetingQuartersContent.ContactPerson,
                    ContactPersonNumber = billetingQuartersContent.ContactPersonNumber
                };

                _context.BilletingQuarters.Add(billetingQuarters);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBilletingQuarters), new { id = billetingQuarters.ID }, billetingQuartersContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("BilletingQuarter")]
        public async Task<ActionResult<IEnumerable<BilletingQuartersDTO.BilletingQuarters.BilletingQuartersContent>>> GetBilletingQuarters()
        {
            try
            {
                var sports = await _context.BilletingQuarters.AsNoTracking().ToListAsync();
                return Ok(sports);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("BilletingQuarter/{id}")]
        public async Task<IActionResult> UpdateBilletingQuarter(int id, BilletingQuartersDTO.BilletingQuarters.BilletingQuartersContent billetingQuartersContent)
        {
            if (id != billetingQuartersContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var billetingQuarters = new BilletingQuarters
                {
                    ID = billetingQuartersContent.ID,
                    RegionID = billetingQuartersContent.RegionID,
                    Address = billetingQuartersContent.Address,
                    Latitude = billetingQuartersContent.Latitude,
                    Longitude = billetingQuartersContent.Longitude,
                    ContactPerson = billetingQuartersContent.ContactPerson,
                    ContactPersonNumber = billetingQuartersContent.ContactPersonNumber
                };

                _context.BilletingQuarters.Attach(billetingQuarters);
                _context.Entry(billetingQuarters).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.BilletingQuarters.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("BilletingQuarter/{id}")]
        public async Task<IActionResult> DeleteBilletingQuarter(int id)
        {
            var billetingQuarters = await _context.BilletingQuarters.FindAsync(id);
            if (billetingQuarters == null)
            {
                return NotFound($"Sport with ID {id} not found");
            }

            _context.BilletingQuarters.Remove(billetingQuarters);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
