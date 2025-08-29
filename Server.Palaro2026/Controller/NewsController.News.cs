using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class NewsController : ControllerBase
    {
        // mapping News to NewsDTO

        private static NewsDTO.News NewsDTOMapper(News news) =>
            new NewsDTO.News
            {
                ID = news.ID,
                NewsCategoryID = news.NewsCategoryID,
                AuthorID = news.AuthorID,
                Title = news.Title,
                Content = news.Content,
                Excerpt = news.Excerpt,
                DateCreated = news.DateCreated,
                IsPublished = news.IsPublished,
                DatePublished = news.DatePublished,
                IsArchived = news.IsArchived
            };

        [HttpGet] // /api/News
        public async Task<ActionResult<IEnumerable<NewsDTO.News>>> GetNews(
        [FromQuery] string? id = null,
        [FromQuery] string? title = null,
        [FromQuery] int? newsCategoryID = null,
        [FromQuery] string? authorID = null,
        [FromQuery] DateTime? dateCreated = null)
        {
            var query = _context.News.AsQueryable();

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.ID!.Contains(id));

            if (!string.IsNullOrEmpty(title))
                query = query.Where(x => x.Title!.Contains(title));

            if (newsCategoryID.HasValue)
                query = query.Where(x => x.NewsCategoryID == newsCategoryID.Value);

            if (!string.IsNullOrEmpty(authorID))
                query = query.Where(x => x.AuthorID!.Contains(authorID));

            if (dateCreated.HasValue)
                query = query.Where(x => x.DateCreated!.Value.Date == dateCreated.Value.Date);

            return await query
                .Select(x => NewsDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpGet("NewsImages")]
        public IActionResult GetNewsImages([FromQuery] string newsID)
        {
            if (string.IsNullOrWhiteSpace(newsID))
                return BadRequest("Missing news ID.");

            var baseFolder = @"\\192.168.2.210\pgas_attachment\palaro2026\media\news";
            var safeFolder = Path.GetFileName(newsID); // sanitize
            var folderPath = Path.Combine(baseFolder, safeFolder);

            if (!Directory.Exists(folderPath))
                return NotFound("Folder not found.");

            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            // 👇 Get file names only (no full path)
            var imageFiles = Directory
                .GetFiles(folderPath)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Select(f => Path.GetFileName(f))
                .ToList();

            if (!imageFiles.Any())
                return NotFound("No images found.");

            return Ok(imageFiles);
        }

        [HttpPost] // /api/News
        public async Task<ActionResult<News>> PostNews(NewsDTO.News news)
        {
            var newsDTO = new News
            {
                ID = news.ID,
                NewsCategoryID = news.NewsCategoryID,
                AuthorID = news.AuthorID,
                Title = news.Title,
                Content = news.Content,
                Excerpt = news.Excerpt,
                DateCreated = news.DateCreated,
                IsPublished = news.IsPublished,
                DatePublished = news.DatePublished,
                IsArchived = news.IsArchived
            };

            _context.News.Add(newsDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (NewsExist(news.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetNews", new { id = news.ID }, NewsDTOMapper(newsDTO));
        }

        [HttpPut("UploadImages/{newsID}")]
        public async Task<IActionResult> UploadNewsImages([FromRoute] string newsID, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            if (string.IsNullOrWhiteSpace(newsID))
            {
                return BadRequest("News ID is required.");
            }

            try
            {
                // Find specific news
                var news = await _context.News.FirstOrDefaultAsync(r => r.ID == newsID);
                if (news == null)
                {
                    return NotFound($"News with ID '{newsID}' not found.");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var maxSize = 10 * 1024 * 1024; // 10 MB per file

                var basePath = Path.Combine(@"\\192.168.2.210\pgas_attachment\palaro2026\media\news", newsID);

                // ✅ Delete existing folder and its contents
                if (Directory.Exists(basePath))
                {
                    Directory.Delete(basePath, recursive: true); // Deletes folder and all contents
                }

                // Recreate directory after deletion
                Directory.CreateDirectory(basePath);

                var uploadedFiles = new List<object>();

                foreach (var file in files)
                {
                    if (file.Length > maxSize)
                    {
                        return BadRequest($"File {file.FileName} exceeds the 10 MB size limit.");
                    }

                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext))
                    {
                        return BadRequest($"File type not allowed: {file.FileName}");
                    }

                    var sanitizedFileName = string.Concat(Path.GetFileNameWithoutExtension(file.FileName)
                                                            .Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

                    var uniqueFileName = $"{sanitizedFileName}_{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(basePath, uniqueFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedFiles.Add(new
                    {
                        fileName = uniqueFileName,
                        storagePath = fullPath
                    });
                }

                return Ok(new
                {
                    message = $"{uploadedFiles.Count} image(s) uploaded successfully.",
                    files = uploadedFiles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading image(s): {ex.Message}");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchNews(string id, [FromBody] NewsDTO.News updateNews)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();
            if (updateNews == null) return BadRequest();

            // Only update if NOT null → preserves existing values
            if (updateNews.NewsCategoryID != null)
                news.NewsCategoryID = updateNews.NewsCategoryID;

            if (updateNews.AuthorID != null)
                news.AuthorID = updateNews.AuthorID;

            if (updateNews.Title != null)
                news.Title = updateNews.Title;

            if (updateNews.Content != null)
                news.Content = updateNews.Content;

            if (updateNews.Excerpt != null)
                news.Excerpt = updateNews.Excerpt;

            if (updateNews.DateCreated != null)
                news.DateCreated = updateNews.DateCreated;

            if (updateNews.IsPublished != null)
                news.IsPublished = updateNews.IsPublished;

            if (updateNews.DatePublished != null || updateNews.IsPublished == false)
                news.DatePublished = updateNews.DatePublished; // allows clearing if null is sent

            if (updateNews.IsArchived != null)
                news.IsArchived = updateNews.IsArchived;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")] // /api/News/{id}
        public async Task<IActionResult> DeleteNews(string id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            // Get the folder path on NAS
            var basePath = Path.Combine(@"\\192.168.2.210\pgas_attachment\palaro2026\media\news", id);

            // Delete from DB
            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            // Try deleting the NAS folder
            try
            {
                if (Directory.Exists(basePath))
                {
                    Directory.Delete(basePath, recursive: true);
                }
            }
            catch (Exception ex)
            {
                // Optional: log the error or return a custom response
                return StatusCode(500, $"News deleted but failed to delete media folder: {ex.Message}");
            }

            return NoContent();
        }

        private bool NewsExist(string id)
        {
            return _context.News.Any(e => e.ID == id);
        }
    }
}
