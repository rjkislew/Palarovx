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

    {// Map ProfilePlayers to ProfilesDTO.ProfilePlayers
        private static ProfilesDTO.ProfilePlayers ProfilePlayersDTOMapper(ProfilePlayers profilePlayers) =>
           new ProfilesDTO.ProfilePlayers
           {
               ID = profilePlayers.ID,
               FirstName = profilePlayers.FirstName,
               LastName = profilePlayers.LastName,
               SchoolID = profilePlayers.SchoolID,
               SportID = profilePlayers.SportID,
               MiddleInitial = profilePlayers.MiddleInitial,
               Sex = profilePlayers.Sex,
               BirthDate = profilePlayers.BirthDate,
               LRN = profilePlayers.LRN,
               SportCategoryID = profilePlayers.SportCategoryID,
               UploadedBy = profilePlayers.UploadedBy,
               ImagePath = string.IsNullOrEmpty(profilePlayers.ImagePath)
           ? null
           : $"/media/profile/players/{profilePlayers.ImagePath}"
           };

        [HttpGet("Player")] // /api/Profiles/Player
        public async Task<ActionResult<IEnumerable<ProfilesDTO.ProfilePlayers>>> GetProfilePlayers(
        [FromQuery] string? ID = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int? schoolID = null,
        [FromQuery] int? sportID = null,
        [FromQuery] string? middleInitial = null,
        [FromQuery] string? sex = null,
        [FromQuery] DateTime? birthDate = null,
        [FromQuery] string? lrn = null,
        [FromQuery] int? sportCategoryID = null,
        [FromQuery] string? uploadedBy = null,
        [FromQuery] string? imagePath = null)
        {
            var query = _context.ProfilePlayers.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (!string.IsNullOrEmpty(firstName))
                query = query.Where(x => x.FirstName!.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(x => x.LastName!.Contains(lastName));

            if (schoolID.HasValue)
                query = query.Where(x => x.SchoolID == schoolID.Value);

            if (sportCategoryID.HasValue)
                query = query.Where(x => x.SportCategoryID == sportCategoryID.Value);

            if (sportID.HasValue)
                query = query.Where(x => x.SportID == sportID.Value);

            if (!string.IsNullOrEmpty(middleInitial))
                query = query.Where(x => x.MiddleInitial!.Contains(middleInitial));

            if (!string.IsNullOrEmpty(sex))
                query = query.Where(x => x.Sex!.Contains(sex));

            if (birthDate.HasValue)
                query = query.Where(x => x.BirthDate == birthDate.Value);

            if (!string.IsNullOrEmpty(lrn))
                query = query.Where(p => p.LRN != null && p.LRN.Contains(lrn));

            if (!string.IsNullOrEmpty(uploadedBy))
                query = query.Where(x => x.UploadedBy == uploadedBy);

            if (!string.IsNullOrEmpty(imagePath))
                query = query.Where(x => x.ImagePath == imagePath);

            return await query
                .Select(x => ProfilePlayersDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Player")] // /api/Profiles/Player
        public async Task<ActionResult<ProfilePlayers>> PostProfilePlayers(ProfilesDTO.ProfilePlayers profilePlayers)
        {
            var loggedInUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User?.FindFirstValue("sub");

            // Always use the JWT user ID for manual entries (more secure)
            if (!string.IsNullOrEmpty(loggedInUserId))
            {
                profilePlayers.UploadedBy = loggedInUserId;
            }
            else if (string.IsNullOrEmpty(profilePlayers.UploadedBy))
            {
                // Fallback if no JWT and no UploadedBy in DTO
                profilePlayers.UploadedBy = "system";
            }

            var profilePlayersDTO = new ProfilePlayers
            {
                ID = profilePlayers.ID,
                FirstName = profilePlayers.FirstName,
                LastName = profilePlayers.LastName,
                SchoolID = profilePlayers.SchoolID,
                SportID = profilePlayers.SportID,
                MiddleInitial = profilePlayers.MiddleInitial,
                Sex = profilePlayers.Sex,
                BirthDate = profilePlayers.BirthDate,
                LRN = profilePlayers.LRN,
                SportCategoryID = profilePlayers.SportCategoryID,
                UploadedBy = profilePlayers.UploadedBy,
            };

            _context.ProfilePlayers.Add(profilePlayersDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayersExist(profilePlayers.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfilePlayers", new { id = profilePlayers.ID }, ProfilePlayersDTOMapper(profilePlayersDTO));
        }

        [HttpPut("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> PutProfilePlayers(string id, ProfilesDTO.ProfilePlayers profilePlayers)
        {
            if (id != profilePlayers.ID)
            {
                return BadRequest();
            }

            var existingPlayerProfile = await _context.ProfilePlayers.FindAsync(id);
            if (existingPlayerProfile == null)
            {
                return NotFound();
            }

            existingPlayerProfile.FirstName = profilePlayers.FirstName;
            existingPlayerProfile.LastName = profilePlayers.LastName;
            existingPlayerProfile.MiddleInitial = profilePlayers.MiddleInitial;
            existingPlayerProfile.Sex = profilePlayers.Sex;
            existingPlayerProfile.BirthDate = profilePlayers.BirthDate;
            existingPlayerProfile.LRN = profilePlayers.LRN;
            existingPlayerProfile.SchoolID = profilePlayers.SchoolID;
            existingPlayerProfile.SportID = profilePlayers.SportID;
            existingPlayerProfile.SportCategoryID = profilePlayers.SportCategoryID;
            existingPlayerProfile.UploadedBy = profilePlayers.UploadedBy;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayersExist(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPatch("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> PatchProfilePlayers(string id, [FromBody] ProfilesDTO.ProfilePlayers updatedPlayer)
        {
            var existingPlayer = await _context.ProfilePlayers.FindAsync(id);

            if (existingPlayer == null) return NotFound();

            if (updatedPlayer.FirstName != null) existingPlayer.FirstName = updatedPlayer.FirstName;
            if (updatedPlayer.LastName != null) existingPlayer.LastName = updatedPlayer.LastName;
            if (updatedPlayer.SchoolID != null) existingPlayer.SchoolID = updatedPlayer.SchoolID;
            if (updatedPlayer.SportID != null) existingPlayer.SportID = updatedPlayer.SportID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfilePlayersExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (ProfilePlayersExist(updatedPlayer.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Player/{id}")] // /api/Profiles/Player/{id}
        public async Task<IActionResult> DeleteProfilePlayers(string id)
        {
            var ProfilePlayers = await _context.ProfilePlayers.FindAsync(id);
            if (ProfilePlayers == null)
            {
                return NotFound();
            }

            _context.ProfilePlayers.Remove(ProfilePlayers);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if a ProfilePlayer exists by ID
        private bool ProfilePlayersExist(string id)
        {
            return _context.ProfilePlayers.Any(e => e.ID == id);
        }

    [HttpPut("Player/UploadAttachment/{id}")]
        public async Task<IActionResult> UploadPlayerImage(string id, [FromForm] IFormFile? attachmentFile)
        {
            if (attachmentFile == null || attachmentFile.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            try
            {
                // 1. Check if event exists
                var player = await _context.ProfilePlayers.FirstOrDefaultAsync(p => p.ID == id);
                if (player == null)
                {
                    return NotFound($"Player with ID '{id}' not found.");
                }

                // 2. Validate extension
                var allowedExtensions = new[] { ".jpeg", ".jpg", ".png"};
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
                var basePath = @"D:\pgas_attachment\palaro2026\media\profile\players";
                //var basePath = @"\\192.168.2.210\pgas_attachment\palaro2026\media\events\official event records";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                // 5. File name is just the Event ID
                string Sanitize(string value) =>
                    string.Concat(value.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

                var sanitizedFileName = $"{Sanitize(player.ID)}{fileExtension}";
                var fullPath = Path.Combine(basePath, sanitizedFileName);

                // 6. Delete old file if exists
                foreach (var file in Directory.GetFiles(basePath, $"{Sanitize(player.ID)}.*"))
                {
                    System.IO.File.Delete(file);
                }

                // 7. Save new file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }

                player.ImagePath = sanitizedFileName;
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
