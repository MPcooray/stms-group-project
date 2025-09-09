using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace STMS.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]      // keep open while testing
    [Route("api")]        // full paths on actions
    public class PlayerEventsController : ControllerBase
    {
        private readonly StmsDbContext _db;
        public PlayerEventsController(StmsDbContext db) => _db = db;

        public record PlayerEventDto(int Id, int PlayerId, string Event);

        public class PlayerEventUpsertDto
        {
            [Required, MaxLength(120)]
            public string Event { get; set; } = "";
        }

        // LIST events for a player
        // GET /api/players/{playerId}/events
        [HttpGet("players/{playerId:int}/events")]
        public async Task<ActionResult<IEnumerable<PlayerEventDto>>> ListByPlayer(int playerId)
        {
            var pExists = await _db.Players.AsNoTracking().AnyAsync(p => p.Id == playerId);
            if (!pExists) return NotFound(new { error = "Player not found" });

            var list = await _db.PlayerEvents.AsNoTracking()
                .Where(pe => pe.PlayerId == playerId)
                .OrderBy(pe => pe.Event)
                .Select(pe => new PlayerEventDto(pe.Id, pe.PlayerId, pe.Event))
                .ToListAsync();

            return Ok(list);
        }

        // CREATE event registration for a player
        // POST /api/players/{playerId}/events
        [HttpPost("players/{playerId:int}/events")]
        public async Task<ActionResult<PlayerEventDto>> Create(int playerId, [FromBody] PlayerEventUpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var pExists = await _db.Players.AsNoTracking().AnyAsync(p => p.Id == playerId);
            if (!pExists) return NotFound(new { error = "Player not found" });

            var ev = body.Event.Trim();

            var dup = await _db.PlayerEvents.AsNoTracking()
                .AnyAsync(pe => pe.PlayerId == playerId && pe.Event == ev);
            if (dup) return Conflict(new { error = "Player already registered for this event" });

            var pe = new PlayerEvent { PlayerId = playerId, Event = ev };
            _db.PlayerEvents.Add(pe);
            await _db.SaveChangesAsync();

            var dto = new PlayerEventDto(pe.Id, pe.PlayerId, pe.Event);
            return CreatedAtAction(nameof(GetById), new { id = pe.Id }, dto);
        }

        // GET one event registration
        // GET /api/player-events/{id}
        [HttpGet("player-events/{id:int}")]
        public async Task<ActionResult<PlayerEventDto>> GetById(int id)
        {
            var pe = await _db.PlayerEvents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return pe is null ? NotFound() : Ok(new PlayerEventDto(pe.Id, pe.PlayerId, pe.Event));
        }

        // UPDATE event name (rare, but handy)
        // PUT /api/player-events/{id}
        [HttpPut("player-events/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PlayerEventUpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var pe = await _db.PlayerEvents.FirstOrDefaultAsync(x => x.Id == id);
            if (pe is null) return NotFound();

            var newEvent = body.Event.Trim();
            var dup = await _db.PlayerEvents.AsNoTracking()
                .AnyAsync(x => x.PlayerId == pe.PlayerId && x.Event == newEvent && x.Id != id);
            if (dup) return Conflict(new { error = "Player already registered for this event" });

            pe.Event = newEvent;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE event registration
        // DELETE /api/player-events/{id}
        [HttpDelete("player-events/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pe = await _db.PlayerEvents.FirstOrDefaultAsync(x => x.Id == id);
            if (pe is null) return NotFound();

            _db.PlayerEvents.Remove(pe);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
