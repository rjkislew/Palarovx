using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class NewsController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public NewsController(Palaro2026Context context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------------------------------------------------------

        // News Details view

        [HttpGet("Details")] // /api/News/Details/
        public async Task<ActionResult<IEnumerable<NewsDTO.NewsDetails.NewsContent>>> GetNewsDetails(
            [FromQuery] string? id = null,
            [FromQuery] string? category = null,
            [FromQuery] string? author = null,
            [FromQuery] string? title = null,
            [FromQuery] DateTime? dateCreated = null,
            [FromQuery] bool? isPublished = null,
            [FromQuery] DateTime? datePublished = null,
            [FromQuery] bool? isArchived = null)
        {
            var query = _context.News
                .Include(x => x.NewsCategory)
                .Include(x => x.Author)
                .AsQueryable();
            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.ID!.Contains(id));
            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.NewsCategory!.Category!.Contains(category));
            if (!string.IsNullOrEmpty(author))
                query = query.Where(x => x.Author!.FirstName!.Contains(author));
            if (!string.IsNullOrEmpty(title))
                query = query.Where(x => x.Title!.Contains(title));
            if (dateCreated.HasValue)
                query = query.Where(x => x.DateCreated!.Value.Date == dateCreated.Value.Date);
            if (datePublished.HasValue)
                query = query.Where(x => x.DatePublished!.Value.Date == datePublished.Value.Date);
            if (isArchived.HasValue)
                query = query.Where(x => x.IsArchived == isArchived);
            return await query
                .OrderByDescending(x => x.DateCreated)
                .Select(x => new NewsDTO.NewsDetails.NewsContent
                {
                    ID = x.ID,
                    Category = x.NewsCategory!.Category,
                    Author = x.Author!.FirstName + " " + x.Author!.LastName,
                    Title = x.Title,
                    Content = x.Content,
                    Excerpt = x.Excerpt,
                    DateCreated = x.DateCreated,
                    IsPublished = x.IsPublished,
                    DatePublished = x.DatePublished,
                    IsArchived = x.IsArchived

                })
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
