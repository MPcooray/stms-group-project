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
        public record PlayerForEventDto(int PlayerId, string PlayerName, int UniversityId, string UniversityName);

        public class PlayerEventUpsertDto
        {
            [Required, MaxLength(120)]
            public string Event { get; set; } = "";
        }

        public class RegisterPlayerBody
        {
            [Required]
            public int PlayerId { get; set; }
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

        // LIST registrations for a tournament event (by tournament + eventId)
        // GET /api/tournaments/{tournamentId}/events/{eventId}/registrations
        [HttpGet("tournaments/{tournamentId:int}/events/{eventId:int}/registrations")]
        public async Task<ActionResult<IEnumerable<PlayerForEventDto>>> ListRegistrationsForTournamentEvent(int tournamentId, int eventId)
        {
            var ev = await _db.TournamentEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev is null || ev.TournamentId != tournamentId)
                return NotFound(new { error = "Event not found in this tournament" });

            var list = await (from pe in _db.PlayerEvents.AsNoTracking()
                              join p in _db.Players.AsNoTracking() on pe.PlayerId equals p.Id
                              join u in _db.Universities.AsNoTracking() on p.UniversityId equals u.Id
                              where u.TournamentId == tournamentId && pe.Event == ev.Name
                              orderby p.Name
                              select new PlayerForEventDto(p.Id, p.Name, u.Id, u.Name))
                              .ToListAsync();
            return Ok(list);
        }

        // REGISTER a player to a tournament event
        // POST /api/tournaments/{tournamentId}/events/{eventId}/registrations { playerId }
        [HttpPost("tournaments/{tournamentId:int}/events/{eventId:int}/registrations")]
        public async Task<IActionResult> RegisterPlayerToTournamentEvent(int tournamentId, int eventId, [FromBody] RegisterPlayerBody body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var ev = await _db.TournamentEvents.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev is null || ev.TournamentId != tournamentId)
                return NotFound(new { error = "Event not found in this tournament" });

            var player = await _db.Players.Include(p => p.University).FirstOrDefaultAsync(p => p.Id == body.PlayerId);
            if (player is null) return NotFound(new { error = "Player not found" });
            if (player.University is null || player.University.TournamentId != tournamentId)
                return BadRequest(new { error = "Player does not belong to this tournament" });

            var exists = await _db.PlayerEvents.AsNoTracking()
                .AnyAsync(pe => pe.PlayerId == body.PlayerId && pe.Event == ev.Name);
            if (exists) return Conflict(new { error = "Player already registered for this event" });

            var pe = new PlayerEvent { PlayerId = body.PlayerId, Event = ev.Name };
            _db.PlayerEvents.Add(pe);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // UNREGISTER a player from a tournament event
        // DELETE /api/tournaments/{tournamentId}/events/{eventId}/registrations/{playerId}
        [HttpDelete("tournaments/{tournamentId:int}/events/{eventId:int}/registrations/{playerId:int}")]
        public async Task<IActionResult> UnregisterPlayerFromTournamentEvent(int tournamentId, int eventId, int playerId)
        {
            var ev = await _db.TournamentEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev is null || ev.TournamentId != tournamentId)
                return NotFound(new { error = "Event not found in this tournament" });

            var pe = await _db.PlayerEvents.FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Event == ev.Name);
            if (pe is null) return NotFound();

            _db.PlayerEvents.Remove(pe);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
