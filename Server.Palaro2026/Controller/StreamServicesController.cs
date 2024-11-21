using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamServicesController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        /// 
        /// 
        /// VIEWS
        /// 
        /// 

        [HttpGet("StreamDetails")]
        public async Task<ActionResult<IEnumerable<StreamServicesDTO.SteamDetails.SD_StreamServicesContent>>> GetStreamDetails()
        {
            try
            {
                // Fetch the data from the database
                var billetingQuarters = await _context.StreamDetails
                    .AsNoTracking()
                    .ToListAsync();

                // Group the sports by category
                var groupedBilletingQuarters = billetingQuarters
                    .GroupBy(r => new { r.StreamServiceID, r.StreamService })
                    .Select(stream => new StreamServicesDTO.SteamDetails.SD_StreamServicesContent
                    {
                        StreamServiceID = stream.Key.StreamServiceID,
                        StreamService = stream.Key.StreamService,
                        StreamURLList = stream
                        .Select(streamURL => new StreamServicesDTO.SteamDetails.SD_StreamURLsContent
                        {
                            StreamURLID = streamURL.StreamURLID,
                            StreamURL = streamURL.StreamURL,
                            Date = streamURL.Date,
                            IsFinished = streamURL.IsFinished,
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

        /*
        [HttpGet("StreamURLDetails")]
        public async Task<ActionResult<IEnumerable<StreamServicesDTO.SteamDetails.SD_StreamServicesContent>>> GetStreamURLDetails(
        [FromQuery] string? streamService = null)
        {
            try
            {
                // Fetch the data from the database
                var streamURLQuery = _context.StreamDetails.AsNoTracking();

                // Apply filters if parameters are provided
                if (!string.IsNullOrEmpty(streamService))
                {
                    streamURLQuery = streamURLQuery.Where(s => s.StreamService == streamService);
                }

                var streamURL = await streamURLQuery.ToListAsync();

                // Group the streams by service and get the latest unfinished URL for each service
                var groupedStreamURLDetails = streamURL
                    .GroupBy(r => r.StreamService)
                    .Select(stream => new StreamServicesDTO.SteamDetails.SD_StreamServicesContent
                    {
                        StreamService = stream.Key,
                        StreamURLList = stream
                            .Where(streamURL => !streamURL.IsFinished) // Filter only unfinished streams
                            .OrderByDescending(streamURL => streamURL.Date) // Order by Date descending
                            .Take(1) // Get the latest URL
                            .Select(streamURL => new StreamServicesDTO.SteamDetails.SD_StreamURLsContent
                            {
                                StreamURL = streamURL.StreamURL ?? string.Empty, // Ensure non-null StreamURL
                                Date = streamURL.Date ?? DateTime.MinValue,     // Ensure non-null Date
                                IsFinished = streamURL.IsFinished    // Ensure non-null IsFinished
                            }).ToList()
                    }).ToList();

                // Return the grouped data
                return Ok(groupedStreamURLDetails);
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
        */
        /// 
        /// 
        /// STREAM SERVICE
        /// 
        /// 

        // Create
        [HttpPost("StreamService")]
        public async Task<ActionResult<StreamServicesDTO.StreamServices.StreamServiceContent>> CreateStreamService([FromBody] StreamServicesDTO.StreamServices.StreamServiceContent streamServiceContent)
        {
            try
            {
                var streamService = new StreamServices
                {
                    ID = streamServiceContent.ID,
                    StreamService = streamServiceContent.StreamService,
                };

                _context.StreamServices.Add(streamService);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStreamServices), new { id = streamService.ID }, streamServiceContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("StreamService")]
        public async Task<ActionResult<IEnumerable<StreamServicesDTO.StreamServices.StreamServiceContent>>> GetStreamServices()
        {
            try
            {
                var streamService = await _context.StreamServices.AsNoTracking().ToListAsync();
                return Ok(streamService);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("StreamService/{id}")]
        public async Task<IActionResult> UpdateStreamService(int id, StreamServicesDTO.StreamServices.StreamServiceContent streamServiceContent)
        {
            if (id != streamServiceContent.ID)
            {
                return BadRequest("Stream Service ID mismatch");
            }

            try
            {
                var streamService = new StreamServices
                {
                    ID = streamServiceContent.ID,
                    StreamService = streamServiceContent.StreamService,
                };

                _context.StreamServices.Attach(streamService);
                _context.Entry(streamService).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Venues.Any(e => e.ID == id))
                {
                    return NotFound($"Stream Service with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("StreamService/{id}")]
        public async Task<IActionResult> DeleteStreamService(int id)
        {
            var streamService = await _context.StreamServices.FindAsync(id);
            if (streamService == null)
            {
                return NotFound($"Stream Service with ID {id} not found");
            }

            _context.StreamServices.Remove(streamService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// 
        /// 
        /// STREAM URL
        /// 
        /// 

        // Create
        [HttpPost("StreamURL")]
        public async Task<ActionResult<StreamServicesDTO.StreamURLs.StreamURLContent>> CreateStreamURL([FromBody] StreamServicesDTO.StreamURLs.StreamURLContent streamURLContent)
        {
            try
            {
                var streamURL = new StreamURLs
                {
                    ID = streamURLContent.ID,
                    StreamServiceID = streamURLContent.StreamServiceID,
                    Date = streamURLContent.Date,
                    StreamURL = streamURLContent.StreamURL,
                    IsFinished = streamURLContent.IsFinished
                };

                _context.StreamURLs.Add(streamURL);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStreamURLs), new { id = streamURL.ID }, streamURLContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("StreamURL")]
        public async Task<ActionResult<IEnumerable<StreamServicesDTO.StreamURLs.StreamURLContent>>> GetStreamURLs()
        {
            try
            {
                var streamURL = await _context.StreamURLs.AsNoTracking().ToListAsync();
                return Ok(streamURL);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("StreamURL/{id}")]
        public async Task<IActionResult> UpdateStreamURL(int id, StreamServicesDTO.StreamURLs.StreamURLContent streamURLContent)
        {
            if (id != streamURLContent.ID)
            {
                return BadRequest("Stream URL ID mismatch");
            }

            try
            {
                var streamURL = new StreamURLs
                {
                    ID = streamURLContent.ID,
                    StreamServiceID = streamURLContent.StreamServiceID,
                    Date = streamURLContent.Date,
                    StreamURL = streamURLContent.StreamURL,
                    IsFinished = streamURLContent.IsFinished
                };

                _context.StreamURLs.Attach(streamURL);
                _context.Entry(streamURL).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Venues.Any(e => e.ID == id))
                {
                    return NotFound($"Stream URL with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("StreamURL/{id}")]
        public async Task<IActionResult> DeleteStreamURL(int id)
        {
            var streamURL = await _context.StreamURLs.FindAsync(id);
            if (streamURL == null)
            {
                return NotFound($"Stream URL with ID {id} not found");
            }

            _context.StreamURLs.Remove(streamURL);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
