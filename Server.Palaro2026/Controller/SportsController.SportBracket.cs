using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    public partial class SportsController : ControllerBase
    {
        private static SportsDTO.SportsBracket MapToDTO(SportsBracket bracket) =>
            new SportsDTO.SportsBracket
            {
                ID = bracket.ID,
                SportID = bracket.SportID,
                BracketName = bracket.BracketName,
                SportName = bracket.Sport != null ? bracket.Sport.Sport : null,
                RegionsList = bracket.SportsBracketRegions?.Select(r => new SportsDTO.SportsBracketRegions
                {
                    ID = r.ID,
                    BracketID = r.BracketID,
                    RegionID = r.RegionID,
                    RegionName = r.Region != null ? r.Region.Region : null,
                    RegionAbbreviation = r.Region != null ? r.Region.Abbreviation : null
                }).ToList()
            };

        // 
        [HttpGet("SportsBracket/{id}")]
        public async Task<ActionResult<SportsDTO.SportsBracket>> GetBracket(int id)
        {
            var bracket = await _context.SportsBracket
                .Include(b => b.Sport)
                .Include(b => b.SportsBracketRegions)
                    .ThenInclude(r => r.Region)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ID == id);

            if (bracket == null)
                return NotFound();

            return Ok(new SportsDTO.SportsBracket
            {
                ID = bracket.ID,
                SportID = bracket.SportID,
                BracketName = bracket.BracketName,
                SportName = bracket.Sport?.Sport,
                RegionsList = bracket.SportsBracketRegions?.Select(r => new SportsDTO.SportsBracketRegions
                {
                    ID = r.ID,
                    BracketID = r.BracketID,
                    RegionID = r.RegionID,
                    RegionName = r.Region?.Region,
                    RegionAbbreviation = r.Region?.Abbreviation
                }).ToList()
            });
        }

        // POST: api/SportsBracket
        [HttpPost("SportsBracket")]
        public async Task<ActionResult<SportsDTO.SportsBracket>> PostBracket(SportsDTO.SportsBracket bracketDTO)
        {
            if (bracketDTO == null)
                return BadRequest("Invalid bracket data.");

            var newBracket = new SportsBracket
            {
                BracketName = bracketDTO.BracketName,
                SportID = bracketDTO.SportID,
                SportsBracketRegions = bracketDTO.RegionsList?.Select(r => new SportsBracketRegions
                {
                    RegionID = r.RegionID
                }).ToList()
            };

            _context.SportsBracket.Add(newBracket);
            await _context.SaveChangesAsync();

            // Map back to DTO with generated IDs
            var createdDTO = new SportsDTO.SportsBracket
            {
                ID = newBracket.ID,
                BracketName = newBracket.BracketName,
                SportID = newBracket.SportID,
                RegionsList = newBracket.SportsBracketRegions?.Select(r => new SportsDTO.SportsBracketRegions
                {
                    ID = r.ID,
                    BracketID = r.BracketID,
                    RegionID = r.RegionID
                }).ToList()
            };

            return CreatedAtAction(nameof(GetBracket), new { id = newBracket.ID }, createdDTO);
        }


        // PUT: api/SportsBracket/{id}
        [HttpPut("SportsBracket/{id}")]
        public async Task<IActionResult> PutBracket(int id, SportsDTO.SportsBracket bracketDTO)
        {
            var existing = await _context.SportsBracket
                .Include(b => b.SportsBracketRegions)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (existing == null)
                return NotFound();

            existing.BracketName = bracketDTO.BracketName;
            existing.SportID = bracketDTO.SportID;

            // Remove old regions
            _context.SportsBracketRegions.RemoveRange(existing.SportsBracketRegions);

            // Add new regions
            if (bracketDTO.RegionsList != null && bracketDTO.RegionsList.Any())
            {
                existing.SportsBracketRegions = bracketDTO.RegionsList
                    .Select(r => new SportsBracketRegions
                    {
                        BracketID = id,
                        RegionID = r.RegionID
                    }).ToList();
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("SportsBracket/{id}")]
        public async Task<IActionResult> DeleteBracket(int id)
        {
            var bracket = await _context.SportsBracket
                .Include(b => b.SportsBracketRegions)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (bracket == null)
                return NotFound();

            // Remove related regions first
            if (bracket.SportsBracketRegions != null && bracket.SportsBracketRegions.Any())
            {
                _context.SportsBracketRegions.RemoveRange(bracket.SportsBracketRegions);
            }

            // Remove main bracket
            _context.SportsBracket.Remove(bracket);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 success
        }

        // GET: api/SportsBracketWithRegions
        [HttpGet("SportsBracketWithRegions")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportsBracket>>> GetBracketsWithRegions()
        {
            var brackets = await _context.SportsBracket
                .Include(b => b.Sport)
                .Include(b => b.SportsBracketRegions)
                    .ThenInclude(r => r.Region)
                .AsNoTracking()
                .ToListAsync();

            var result = brackets.Select(b => new SportsDTO.SportsBracket
            {
                ID = b.ID,
                SportID = b.SportID,
                BracketName = b.BracketName,
                SportName = b.Sport?.Sport,
                RegionsList = b.SportsBracketRegions?.Select(r => new SportsDTO.SportsBracketRegions
                {
                    ID = r.ID,
                    BracketID = r.BracketID,
                    RegionID = r.RegionID,
                    RegionName = r.Region?.Region,
                    RegionAbbreviation = r.Region?.Abbreviation
                }).ToList()
            });

            return Ok(result);
        }
    }
}

