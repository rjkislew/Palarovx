using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;
using Server.Palaro2026.DTO;
namespace Server.Palaro2026.Controller
{
    public partial class SchoolsController : ControllerBase
    {
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
                var allowedExtensions = new[] { ".webp" };
                var fileExtension = Path.GetExtension(logoFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid logo file type. Only .webp is allowed.");
                }

                // 3. Validate file size
                if (logoFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Logo file size exceeds the 5 MB limit.");
                }

                // 4. Path to logos
                var basePath = @"\\192.168.2.210\pgas_attachment\palaro2026\media\regions\region_logo";
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
                var allowedExtensions = new[] { ".webp" };
                var fileExtension = Path.GetExtension(logoFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid logo file type. Only .webp is allowed.");
                }

                // 3. Validate file size
                if (logoFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Logo file size exceeds the 5 MB limit.");
                }

                // 4. Path to logos
                var basePath = @"\\192.168.2.210\pgas_attachment\palaro2026\media\regions\team_logo";
                //var basePath = @"D:\pgas_attachment\palaro2026\media\regions\team_logo";
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
