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
    [Route("api")]
    public class PlayersController : ControllerBase
    {
        private readonly StmsDbContext _db;
        public PlayersController(StmsDbContext db) => _db = db;

        public record PlayerDto(int Id, string Name, int UniversityId, int? Age);

        public class PlayerUpsertDto
        {
            [Required, MaxLength(160)]
            public string Name { get; set; } = "";

            [Range(1, 120)]
            public int? Age { get; set; }
        }

        // GET /api/universities/{universityId}/players
        [HttpGet("universities/{universityId:int}/players")]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> ListByUniversity(int universityId)
        {
            var uExists = await _db.Universities.AsNoTracking().AnyAsync(u => u.Id == universityId);
            if (!uExists) return NotFound(new { error = "University not found" });

            var list = await _db.Players.AsNoTracking()
                .Where(p => p.UniversityId == universityId)
                .OrderBy(p => p.Name)
                .Select(p => new PlayerDto(p.Id, p.Name, p.UniversityId, p.Age))
                .ToListAsync();

            return Ok(list);
        }

        // POST /api/universities/{universityId}/players
        [HttpPost("universities/{universityId:int}/players")]
        public async Task<ActionResult<PlayerDto>> Create(int universityId, [FromBody] PlayerUpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var uExists = await _db.Universities.AsNoTracking().AnyAsync(u => u.Id == universityId);
            if (!uExists) return NotFound(new { error = "University not found" });

            var name = body.Name.Trim();

            var dup = await _db.Players.AsNoTracking()
                .AnyAsync(p => p.UniversityId == universityId && p.Name == name);
            if (dup) return Conflict(new { error = "Player already exists in this university" });

            var p = new Player { Name = name, UniversityId = universityId, Age = body.Age };
            _db.Players.Add(p);
            await _db.SaveChangesAsync();

            var dto = new PlayerDto(p.Id, p.Name, p.UniversityId, p.Age);
            return CreatedAtAction(nameof(GetById), new { id = p.Id }, dto);
        }

        // GET /api/players/{id}
        [HttpGet("players/{id:int}")]
        public async Task<ActionResult<PlayerDto>> GetById(int id)
        {
            var p = await _db.Players.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return p is null ? NotFound() : Ok(new PlayerDto(p.Id, p.Name, p.UniversityId, p.Age));
        }

        // PUT /api/players/{id}
        [HttpPut("players/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PlayerUpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var p = await _db.Players.FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound();

            var newName = body.Name.Trim();

            var dup = await _db.Players.AsNoTracking().AnyAsync(x =>
                x.UniversityId == p.UniversityId && x.Name == newName && x.Id != id);
            if (dup) return Conflict(new { error = "Player already exists in this university" });

            p.Name = newName;
            p.Age = body.Age;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/players/{id}
        [HttpDelete("players/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Players.FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound();

            _db.Players.Remove(p);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
