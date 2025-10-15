using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace STMS.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api")] // full paths on actions
    public class TournamentEventsController : ControllerBase
    {
        private readonly StmsDbContext _db;
        public TournamentEventsController(StmsDbContext db) => _db = db;
        // GET /api/events/{eventId}/results

        public record EventDto(int Id, int TournamentId, string Name);
        public record RegisteredEventDto(string Name, int Registrations);
        public class UpsertDto { [Required, MaxLength(120)] public string Name { get; set; } = string.Empty; }

        // GET /api/tournaments/{tournamentId}/events
        [HttpGet("tournaments/{tournamentId:int}/events")]
        public async Task<ActionResult<IEnumerable<EventDto>>> ListByTournament(int tournamentId)
        {
            var exists = await _db.Tournaments.AsNoTracking().AnyAsync(t => t.Id == tournamentId);
            if (!exists) return NotFound(new { error = "Tournament not found" });

            var list = await _db.TournamentEvents.AsNoTracking()
                .Where(e => e.TournamentId == tournamentId)
                .OrderBy(e => e.Name)
                .Select(e => new EventDto(e.Id, e.TournamentId, e.Name))
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/tournaments/{tournamentId}/registered-events
        // Distinct set of event names players registered for in this tournament (+ counts)
        [HttpGet("tournaments/{tournamentId:int}/registered-events")]
        public async Task<ActionResult<IEnumerable<RegisteredEventDto>>> ListRegisteredByTournament(int tournamentId)
        {
            var exists = await _db.Tournaments.AsNoTracking().AnyAsync(t => t.Id == tournamentId);
            if (!exists) return NotFound(new { error = "Tournament not found" });

            var list = await (from pe in _db.PlayerEvents.AsNoTracking()
                              join p in _db.Players.AsNoTracking() on pe.PlayerId equals p.Id
                              join u in _db.Universities.AsNoTracking() on p.UniversityId equals u.Id
                              where u.TournamentId == tournamentId
                              group pe by pe.Event into g
                              orderby g.Key
                              select new RegisteredEventDto(g.Key, g.Count()))
                             .ToListAsync();

            return Ok(list);
        }

        // POST /api/tournaments/{tournamentId}/events
        [HttpPost("tournaments/{tournamentId:int}/events")]
        public async Task<ActionResult<EventDto>> Create(int tournamentId, [FromBody] UpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var exists = await _db.Tournaments.AsNoTracking().AnyAsync(t => t.Id == tournamentId);
            if (!exists) return NotFound(new { error = "Tournament not found" });

            var name = body.Name.Trim();
            var dup = await _db.TournamentEvents.AsNoTracking().AnyAsync(x => x.TournamentId == tournamentId && x.Name == name);
            if (dup) return Conflict(new { error = "Event already exists in this tournament" });

            var ev = new TournamentEvent { TournamentId = tournamentId, Name = name };
            _db.TournamentEvents.Add(ev);
            await _db.SaveChangesAsync();

            var dto = new EventDto(ev.Id, ev.TournamentId, ev.Name);
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, dto);
        }

        // GET /api/tournament-events/{id}
        [HttpGet("tournament-events/{id:int}")]
        public async Task<ActionResult<EventDto>> GetById(int id)
        {
            var ev = await _db.TournamentEvents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return ev is null ? NotFound() : Ok(new EventDto(ev.Id, ev.TournamentId, ev.Name));
        }

        // PUT /api/tournament-events/{id}
        [HttpPut("tournament-events/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var ev = await _db.TournamentEvents.FirstOrDefaultAsync(x => x.Id == id);
            if (ev is null) return NotFound();

            var newName = body.Name.Trim();
            var dup = await _db.TournamentEvents.AsNoTracking().AnyAsync(x => x.TournamentId == ev.TournamentId && x.Name == newName && x.Id != id);
            if (dup) return Conflict(new { error = "Event already exists in this tournament" });

            ev.Name = newName;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/tournament-events/{id}
        [HttpDelete("tournament-events/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.TournamentEvents.FirstOrDefaultAsync(x => x.Id == id);
            if (ev is null) return NotFound();
            _db.TournamentEvents.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET /api/events/{eventId}/results
        [HttpGet("events/{eventId:int}/results")]
        public async Task<ActionResult<IEnumerable<object>>> GetEventResults(int eventId)
        {
            var eventExists = await _db.TournamentEvents.AsNoTracking().AnyAsync(e => e.Id == eventId);
            if (!eventExists) return NotFound(new { error = "Event not found" });

            var timings = await _db.Timings
                    .Where(t => t.EventId == eventId)
                .OrderBy(t => t.TimeMs)
                .ToListAsync();

            var playerIds = timings.Select(t => t.PlayerId).Distinct().ToList();
            var players = await _db.Players
                .Where(p => playerIds.Contains(p.Id))
                .Include(p => p.University)
                .ToListAsync();

            // We need to assign ranks only to timings with TimeMs > 0. Timings with TimeMs <= 0
            // should remain in the returned list but have a null rank and zero points.
            // Place valid timings (TimeMs > 0) first ordered ascending by time,
            // then place timings with TimeMs <= 0 at the end so they never appear first.
            var ordered = timings
                .OrderBy(t => t.TimeMs <= 0) // false (valid) come before true (invalid)
                .ThenBy(t => t.TimeMs)
                .ToList();
            var rankedList = new List<object>();
            int currentRank = 0;
            foreach (var t in ordered)
            {
                var player = players.FirstOrDefault(p => p.Id == t.PlayerId);
                int? rank = null;
                int points = 0;
                if (t.TimeMs > 0)
                {
                    // only increment rank for valid timings
                    currentRank += 1;
                    rank = currentRank;
                    points = rank switch
                    {
                        1 => 10,
                        2 => 8,
                        3 => 7,
                        4 => 5,
                        5 => 4,
                        6 => 3,
                        7 => 2,
                        8 => 1,
                        _ => 0
                    };
                }

                rankedList.Add(new
                {
                    playerId = t.PlayerId,
                    playerName = player?.Name ?? "",
                    universityName = player?.University?.Name ?? "",
                    timeMs = t.TimeMs,
                    rank,
                    points
                });
            }

            var results = rankedList;

            return Ok(results);
        }
    }
}