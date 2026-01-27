using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using System.Security.Claims;


namespace Server.Palaro2026.Controller
{
    public partial class ProfilesController
    {
        // Map ProfileCoaches to ProfilesDTO.ProfileCoaches
        private static ProfilesDTO.ProfileCoaches ProfileCoachesDTOMapper(ProfileCoaches profileCoaches) =>
           new ProfilesDTO.ProfileCoaches
           {
               ID = profileCoaches.ID,
               FirstName = profileCoaches.FirstName,
               LastName = profileCoaches.LastName,
               MiddleInitial = profileCoaches.MiddleInitial,
               Sex = profileCoaches.Sex,
               BirthDate = profileCoaches.BirthDate,
               Designation = profileCoaches.Designation,
               SportID = profileCoaches.SportID,
               GenderCategoryID = profileCoaches.GenderCategoryID,
               SchoolRegionID = profileCoaches.SchoolRegionID,
               SchoolDivisionID = profileCoaches.SchoolDivisionID,
               SchoolID = profileCoaches.SchoolID,
               SportCategoryID = profileCoaches.SportCategoryID,
               UploadedBy = profileCoaches.UploadedBy,
               ImagePath = string.IsNullOrEmpty(profileCoaches.ImagePath)
           ? null
           : $"/media/profile/players/{profileCoaches.ImagePath}"
           };

        [HttpGet("Coach")] // /api/Profiles/Coach
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfileCoaches>>> GetProfileCoaches(
        [FromQuery] string? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? middleInitial = null,
        [FromQuery] string? sex = null,
        [FromQuery] DateTime? birthDate = null,
        [FromQuery] string? designation = null,
        [FromQuery] int? sportsID = null,
        [FromQuery] int? genderCategoryID = null,
        [FromQuery] int? sportCategoryID = null,
        [FromQuery] int? schoolRegionID = null,
        [FromQuery] int? schoolDivisionID = null,
        [FromQuery] int? schoolID = null,
        [FromQuery] string? uploadedBy = null,
        [FromQuery] string? imagePath = null)
        {
            var query = _context.ProfileCoaches.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName!.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName!.Contains(lastName));
            
            if (!string.IsNullOrEmpty(middleInitial))
                query = query.Where(x => x.MiddleInitial!.Contains(middleInitial));
            
            if (!string.IsNullOrEmpty(sex))
                query = query.Where(x => x.Sex!.Contains(sex));
            
            if (birthDate.HasValue)
                query = query.Where(x => x.BirthDate == birthDate.Value);
            
            if (!string.IsNullOrEmpty(designation))
                query = query.Where(x => x.Designation!.Contains(designation));
            
            if (sportCategoryID.HasValue)
                query = query.Where(x => x.SportCategoryID == sportCategoryID.Value);
            
            if (sportsID.HasValue)
                query = query.Where(x => x.SportID == sportsID.Value);
            
            if (genderCategoryID.HasValue)
                query = query.Where(x => x.GenderCategoryID == genderCategoryID.Value);

            if (schoolRegionID.HasValue)
                query = query.Where(x => x.SchoolRegionID == schoolRegionID.Value);
            
            if (schoolDivisionID.HasValue)
                query = query.Where(x => x.SchoolDivisionID == schoolDivisionID.Value);
            
            if (schoolID.HasValue)
                query = query.Where(x => x.SchoolID == schoolID.Value);

            if (!string.IsNullOrEmpty(uploadedBy))
                query = query.Where(x => x.UploadedBy == uploadedBy);

            if (!string.IsNullOrEmpty(imagePath))
                query = query.Where(x => x.ImagePath == imagePath);

            return await query
                .Select(x => ProfileCoachesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Coach")] // /api/Profiles/Coach
        public async Task<ActionResult<ProfileCoaches>> PostProfileCoaches(ProfilesDTO.ProfileCoaches profileCoaches)
        {
            var loggedInUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User?.FindFirstValue("sub");

            // Always use the JWT user ID for manual entries (more secure)
            if (!string.IsNullOrEmpty(loggedInUserId))
            {
                profileCoaches.UploadedBy = loggedInUserId;
            }
            else if (string.IsNullOrEmpty(profileCoaches.UploadedBy))
            {
                // Fallback if no JWT and no UploadedBy in DTO
                profileCoaches.UploadedBy = "system";
            }

            var profileCoachesDTO = new ProfileCoaches
            {
                ID = profileCoaches.ID,
                FirstName = profileCoaches.FirstName,
                LastName = profileCoaches.LastName,
                MiddleInitial = profileCoaches.MiddleInitial,
                Sex = profileCoaches.Sex,
                BirthDate = profileCoaches.BirthDate,
                Designation = profileCoaches.Designation,
                SportID = profileCoaches.SportID,
                GenderCategoryID = profileCoaches.GenderCategoryID,
                SchoolRegionID = profileCoaches.SchoolRegionID,
                SchoolDivisionID = profileCoaches.SchoolDivisionID,
                SchoolID = profileCoaches.SchoolID,
                SportCategoryID = profileCoaches.SportCategoryID,
                UploadedBy = profileCoaches.UploadedBy,
            };

            _context.ProfileCoaches.Add(profileCoachesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfileCoachesExist(profileCoaches.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfileCoaches", new { id = profileCoaches.ID }, ProfileCoachesDTOMapper(profileCoachesDTO));
        }

        [HttpPut("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> PutProfileCoaches(string id, ProfilesDTO.ProfileCoaches profileCoaches)
        {
            if (id != profileCoaches.ID)
            {
                return BadRequest();
            }

            var existingCoachProfile = await _context.ProfileCoaches.FindAsync(id);
            if (existingCoachProfile == null)
            {
                return NotFound();
            }

            existingCoachProfile.FirstName = profileCoaches.FirstName;
            existingCoachProfile.LastName = profileCoaches.LastName;
            existingCoachProfile.SchoolRegionID = profileCoaches.SchoolRegionID;
            existingCoachProfile.Sex = profileCoaches.Sex;
            existingCoachProfile.Designation = profileCoaches.Designation;
            existingCoachProfile.MiddleInitial = profileCoaches.MiddleInitial;
            existingCoachProfile.BirthDate = profileCoaches.BirthDate;
            existingCoachProfile.SportID = profileCoaches.SportID;
            existingCoachProfile.GenderCategoryID = profileCoaches.GenderCategoryID;
            existingCoachProfile.SchoolDivisionID = profileCoaches.SchoolDivisionID;
            existingCoachProfile.SchoolID = profileCoaches.SchoolID;
            existingCoachProfile.SportCategoryID = profileCoaches.SportCategoryID;
            existingCoachProfile.UploadedBy = profileCoaches.UploadedBy;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileCoachesExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> PatchProfileCoaches(string id, [FromBody] ProfilesDTO.ProfileCoaches updatedProfileCoach)
        {
            var existingCoach = await _context.ProfileCoaches.FindAsync(id);

            if (existingCoach == null) return NotFound();

            if (updatedProfileCoach.FirstName != null) existingCoach.FirstName = updatedProfileCoach.FirstName;
            if (updatedProfileCoach.LastName != null) existingCoach.LastName = updatedProfileCoach.LastName;
            if (updatedProfileCoach.SchoolRegionID != null) existingCoach.SchoolRegionID = updatedProfileCoach.SchoolRegionID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileCoachesExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfileCoachesExist(updatedProfileCoach.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Coach/{id}")] // /api/Profiles/Coach/{id}
        public async Task<IActionResult> DeleteProfileCoaches(string id)
        {
            var ProfileCoaches = await _context.ProfileCoaches.FindAsync(id);
            if (ProfileCoaches == null)
            {
                return NotFound();
            }

            _context.ProfileCoaches.Remove(ProfileCoaches);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a ProfileCoach exist by ID
        private bool ProfileCoachesExist(string id)
        {
            return _context.ProfileCoaches.Any(e => e.ID == id);
        }

        [HttpPut("Coach/UploadAttachment/{id}")]
        public async Task<IActionResult> UploadCoachImage(string id, [FromForm] IFormFile? attachmentFile)
        {
            if (attachmentFile == null || attachmentFile.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            try
            {
                // 1. Check if event exists
                var coach = await _context.ProfileCoaches.FirstOrDefaultAsync(p => p.ID == id);
                if (coach == null)
                {
                    return NotFound($"Player with ID '{id}' not found.");
                }

                // 2. Validate extension
                var allowedExtensions = new[] { ".jpeg", ".jpg", ".png" };
                var fileExtension = Path.GetExtension(attachmentFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Allowed types: .jpeg, .jpg, .png");
                }

                // 3. Validate file size (max 5 MB)
                if (attachmentFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("File size exceeds the 5 MB limit.");
                }

                // 4. Path to save
                //var basePath = @"D:\pgas_attachment\palaro2026\media\profile\coaches";
                var basePath = @"\\192.168.2.210\pgas_attachment\palaro2026\media\profiles\coaches";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                // 5. File name is just the Event ID
                string Sanitize(string value) =>
                    string.Concat(value.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

                var sanitizedFileName = $"{Sanitize(coach.ID)}{fileExtension}";
                var fullPath = Path.Combine(basePath, sanitizedFileName);

                // 6. Delete old file if exists
                foreach (var file in Directory.GetFiles(basePath, $"{Sanitize(coach.ID)}.*"))
                {
                    System.IO.File.Delete(file);
                }

                // 7. Save new file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }

                coach.ImagePath = sanitizedFileName;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Attachment uploaded successfully.",
                    fileName = sanitizedFileName,
                    storagePath = fullPath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading attachment: {ex.Message}");
            }
        }
    }
}
