using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;


namespace Server.Palaro2026.Controller
{
    public partial class EventsController : ControllerBase
    {
        // Map EventStages entity to EventsDTO.EventStages
        private static EventsDTO.EventStages EventStagesDTOMapper(EventStages eventStages) =>
           new EventsDTO.EventStages
           {
               ID = eventStages.ID,
               Stage = eventStages.Stage,
           };

        [HttpGet("Stages")] // /api/Events/Stages
        public async Task<ActionResult<IEnumerable<EventsDTO.EventStages>>> GetEventStages(
        [FromQuery] int? ID = null,
        [FromQuery] string? stage = null)
        {
            var query = _context.EventStages.AsQueryable();

            if (ID.HasValue)
                query = query.Where(x => x.ID == ID.Value);

            if (!string.IsNullOrEmpty(stage))
                query = query.Where(x => x.Stage!.Contains(stage));

            return await query
                .Select(x => EventStagesDTOMapper(x))
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost("Stages")] // /api/Events/Stages
        public async Task<ActionResult<EventStages>> PostEventStages(EventsDTO.EventStages eventStages)
        {
            var eventStagesDTO = new EventStages
            {
                ID = eventStages.ID,
                Stage = eventStages.Stage,
            };

            _context.EventStages.Add(eventStagesDTO);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventStageExist(eventStages.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEventStages", new { id = eventStages.ID }, EventStagesDTOMapper(eventStagesDTO));
        }

        [HttpPut("Stages/{id}")] // /api/Events/Stages/{id}
        public async Task<IActionResult> PutEventStages(int id, EventsDTO.EventStages eventStages)
        {
            if (id != eventStages.ID)
            {
                return BadRequest();
            }

            var existingEventStages = await _context.EventStages.FindAsync(id);
            if (existingEventStages == null)
            {
                return NotFound();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStageExist(id))
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

        [HttpPatch("Stages/{id}")] // /api/Events/Stages/{id}
        public async Task<IActionResult> PatchEventStages(int id, [FromBody] EventsDTO.EventStages updatedEventStage)
        {
            var existingStage = await _context.EventStages.FindAsync(id);

            if (existingStage == null) return NotFound();

            if (updatedEventStage.Stage != null) existingStage.Stage = updatedEventStage.Stage;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventStageExist(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (EventStageExist(updatedEventStage.ID)) return Conflict();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Stages/{id}")] // /api/Events/Stages/{id}
        public async Task<IActionResult> DeleteEventStages(int id)
        {
            var eventStages = await _context.EventStages.FindAsync(id);
            if (eventStages == null)
            {
                return NotFound();
            }

            _context.EventStages.Remove(eventStages);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Check if Event Stage exists
        private bool EventStageExist(int id)
        {
            return _context.EventStages.Any(e => e.ID == id);
        }

    }
}
