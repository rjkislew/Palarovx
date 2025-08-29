using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;


namespace Server.Palaro2026.Controller
{
    public partial class EventsController : ControllerBase
    {
        // Event REST methods

        // Map Events entity to EventsDTO.Events
        private static EventsDTO.Events EventsDTOMapper(Events events) =>
           new EventsDTO.Events
           {
               ID = events.ID,
               EventStageID = events.EventStageID,
               SportSubcategoryID = events.SportSubcategoryID,
               EventVenuesID = events.EventVenuesID,
               Date = events.Date,
               Time = events.Time,
               OnStream = events.OnStream,
               EventStreamID = events.EventStreamID,
               GamePhase = events.GamePhase,
               IsFinished = events.IsFinished,
               Archived = events.Archived,
               Deleted = events.Deleted,
               UserID = events.UserID,
           };


        [HttpGet] // /api/Events
        public async Task<ActionResult<IEnumerable<EventsDTO.Events>>> GetEvents(
        [FromQuery] string? ID = null,
        [FromQuery] int? eventStageID = null,
        [FromQuery] int? sportSubcategoryID = null,
        [FromQuery] int? eventVenuesID = null,
        [FromQuery] DateTime? date = null,
        [FromQuery] TimeSpan? time = null,
        [FromQuery] bool? onStream = null,
        [FromQuery] int? streamID = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] bool? archived = null,
        [FromQuery] bool? deleted = null,
        [FromQuery] string? userID = null)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(ID))
                query = query.Where(x => x.ID == ID);

            if (eventStageID.HasValue)
                query = query.Where(x => x.EventStageID == eventStageID.Value);

            if (sportSubcategoryID.HasValue)
                query = query.Where(x => x.SportSubcategoryID == sportSubcategoryID.Value);

            if (eventVenuesID.HasValue)
                query = query.Where(x => x.EventVenuesID == eventVenuesID.Value);

            if (date.HasValue)
                query = query.Where(x => x.Date == date.Value);

            if (time.HasValue)
                query = query.Where(x => x.Time == time.Value);

            if (onStream.HasValue)
                query = query.Where(x => x.OnStream == onStream.Value);

            if (streamID.HasValue)
                query = query.Where(x => x.EventStreamID == streamID.Value);

            if (isFinished.HasValue)
                query = query.Where(x => x.IsFinished == isFinished.Value);

            if (archived.HasValue)
                query = query.Where(x => x.Archived == archived.Value);

            if (deleted.HasValue)
                query = query.Where(x => x.Deleted == deleted.Value);

            if (!string.IsNullOrEmpty(userID))
                query = query.Where(x => x.UserID == userID);

            return await query
                .Select(x => EventsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost] // /api/Events
        public async Task<ActionResult<Events>> PostEvents(EventsDTO.Events events)
        {
            var eventsDTO = new Events
            {
                ID = events.ID,
                EventStageID = events.EventStageID,
                SportSubcategoryID = events.SportSubcategoryID,
                EventVenuesID = events.EventVenuesID,
                Date = events.Date,
                Time = events.Time,
                OnStream = events.OnStream,
                EventStreamID = events.EventStreamID,
                GamePhase = events.GamePhase,
                IsFinished = events.IsFinished,
                Archived = events.Archived,
                Deleted = events.Deleted,
                UserID = events.UserID,
            };

            _context.Events.Add(eventsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventExist(events.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEvents", new { id = events.ID }, EventsDTOMapper(eventsDTO));
        }

        [HttpPut("{id}")] // /api/Events/{id}
        public async Task<IActionResult> PutEvents(string id, EventsDTO.Events events)
        {
            if (id != events.ID)
            {
                return BadRequest();
            }

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            existingEvent.EventStageID = events.EventStageID;
            existingEvent.GamePhase = events.GamePhase;
            existingEvent.SportSubcategoryID = events.SportSubcategoryID;
            existingEvent.EventVenuesID = events.EventVenuesID;
            existingEvent.Date = events.Date;
            existingEvent.Time = events.Time;
            existingEvent.OnStream = events.OnStream;
            existingEvent.EventStreamID = events.EventStreamID;
            existingEvent.IsFinished = events.IsFinished;
            existingEvent.Archived = events.Archived;
            existingEvent.Deleted = events.Deleted;
            existingEvent.UserID = events.UserID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExist(id))
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

        [HttpPut("UploadAttachment/{id}")]
        public async Task<IActionResult> UploadRegionLogo(string id, [FromForm] IFormFile? attachmentFile)
        {
            if (attachmentFile == null || attachmentFile.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            try
            {
                // 1. Check if event exists
                var events = await _context.Events.FirstOrDefaultAsync(e => e.ID == id);
                if (events == null)
                {
                    return NotFound($"Event with ID '{id}' not found.");
                }

                // 2. Validate extension
                var allowedExtensions = new[] { ".jpeg", ".jpg", ".png", ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(attachmentFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Allowed types: .jpeg, .jpg, .png, .pdf, .doc, .docx");
                }

                // 3. Validate file size (max 5 MB)
                if (attachmentFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("File size exceeds the 5 MB limit.");
                }

                // 4. Path to save
                var basePath = @"\\192.168.2.210\pgas_attachment\palaro2026\media\events\official event records";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                // 5. File name is just the Event ID
                string Sanitize(string value) =>
                    string.Concat(value.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

                var sanitizedFileName = $"{Sanitize(events.ID)}{fileExtension}";
                var fullPath = Path.Combine(basePath, sanitizedFileName);

                // 6. Delete old file if exists
                foreach (var file in Directory.GetFiles(basePath, $"{Sanitize(events.ID)}.*"))
                {
                    System.IO.File.Delete(file);
                }

                // 7. Save new file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }

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

        [HttpPatch("{id}")] // /api/Events/{id}
        public async Task<IActionResult> PatchEvents(string id, [FromBody] EventsDTO.Events updatedEvent)
        {
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null) return NotFound();

            if (updatedEvent.EventStageID != null) existingEvent.EventStageID = updatedEvent.EventStageID;
            if (updatedEvent.SportSubcategoryID != null) existingEvent.SportSubcategoryID = updatedEvent.SportSubcategoryID;
            if (updatedEvent.EventVenuesID != null) existingEvent.EventVenuesID = updatedEvent.EventVenuesID;
            if (updatedEvent.Date != null) existingEvent.Date = updatedEvent.Date;
            if (updatedEvent.Time != null) existingEvent.Time = updatedEvent.Time;
            if (updatedEvent.OnStream != null) existingEvent.OnStream = updatedEvent.OnStream;
            if (updatedEvent.EventStreamID != null) existingEvent.EventStreamID = updatedEvent.EventStreamID;
            if (updatedEvent.GamePhase != null) existingEvent.GamePhase = updatedEvent.GamePhase;
            if (updatedEvent.IsFinished != null) existingEvent.IsFinished = updatedEvent.IsFinished;
            if (updatedEvent.Archived != null) existingEvent.Archived = updatedEvent.Archived;
            if (updatedEvent.Deleted != null) existingEvent.Deleted = updatedEvent.Deleted;
            if (updatedEvent.UserID != null) existingEvent.UserID = updatedEvent.UserID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventExist(updatedEvent.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")] // /api/Events/{id}
        public async Task<IActionResult> DeleteEvents(string id)
        {
            var events = await _context.Events.FindAsync(id);
            if (events == null)
            {
                return NotFound();
            }

            _context.Events.Remove(events);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event exists
        private bool EventExist(string id)
        {
            return _context.Events.Any(e => e.ID == id);
        }
    }
}
