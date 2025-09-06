using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace STMS.Api.Controllers
{
    [ApiController]
    [AllowAnonymous] // keep open during dev; remove when you enable auth
    [Route("api")]   // we'll declare full paths on actions
    public class UniversitiesController : ControllerBase
    {
        private readonly StmsDbContext _db;
        public UniversitiesController(StmsDbContext db) => _db = db;

        public record UniversityDto(int Id, string Name, int TournamentId);
        public class UniversityUpsertDto
        {
            [Required, MaxLength(160)] public string Name { get; set; } = "";
        }

        // GET /api/tournaments/{tournamentId}/universities
        [HttpGet("tournaments/{tournamentId:int}/universities")]
        public async Task<ActionResult<IEnumerable<UniversityDto>>> ListByTournament(int tournamentId)
        {
            var exists = await _db.Tournaments.AsNoTracking().AnyAsync(t => t.Id == tournamentId);
            if (!exists) return NotFound(new { error = "Tournament not found" });

            var list = await _db.Universities.AsNoTracking()
                .Where(u => u.TournamentId == tournamentId)
                .OrderBy(u => u.Name)
                .Select(u => new UniversityDto(u.Id, u.Name, u.TournamentId))
                .ToListAsync();

            return Ok(list);
        }

        // POST /api/tournaments/{tournamentId}/universities
        [HttpPost("tournaments/{tournamentId:int}/universities")]
        public async Task<ActionResult<UniversityDto>> Create(int tournamentId, [FromBody] UniversityUpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var tExists = await _db.Tournaments.AsNoTracking().AnyAsync(t => t.Id == tournamentId);
            if (!tExists) return NotFound(new { error = "Tournament not found" });

            var name = body.Name.Trim();
            var dup = await _db.Universities.AsNoTracking()
                .AnyAsync(u => u.TournamentId == tournamentId && u.Name == name);
            if (dup) return Conflict(new { error = "University already exists in this tournament" });

            var u = new University { Name = name, TournamentId = tournamentId };
            _db.Universities.Add(u);
            await _db.SaveChangesAsync();

            var dto = new UniversityDto(u.Id, u.Name, u.TournamentId);
            return CreatedAtAction(nameof(GetById), new { id = u.Id }, dto);
        }

        // GET /api/universities/{id}
        [HttpGet("universities/{id:int}")]
        public async Task<ActionResult<UniversityDto>> GetById(int id)
        {
            var u = await _db.Universities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return u is null
                ? NotFound()
                : Ok(new UniversityDto(u.Id, u.Name, u.TournamentId));
        }

        // PUT /api/universities/{id}
        [HttpPut("universities/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UniversityUpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var u = await _db.Universities.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();

            var newName = body.Name.Trim();
            var dup = await _db.Universities.AsNoTracking()
                .AnyAsync(x => x.TournamentId == u.TournamentId && x.Name == newName && x.Id != id);
            if (dup) return Conflict(new { error = "University already exists in this tournament" });

            u.Name = newName;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/universities/{id}
        [HttpDelete("universities/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.Universities.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();

            _db.Universities.Remove(u);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
