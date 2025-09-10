using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace STMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/tournaments
    // [Authorize] // enable later once login works end-to-end
    public class TournamentsController : ControllerBase
    {
        private readonly StmsDbContext _db;
        public TournamentsController(StmsDbContext db) => _db = db;

        public record TournamentDto(int Id, string Name, string Venue, DateTime Date, DateTime? EndDate);
        public class UpsertDto
        {
            [Required, MaxLength(120)] public string Name { get; set; } = "";
            [Required, MaxLength(120)] public string Venue { get; set; } = "";
            [Required] public DateTime Date { get; set; }
            public DateTime? EndDate { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TournamentDto>>> GetAll()
        {
            var list = await _db.Tournaments.OrderByDescending(t => t.Id)
                .Select(t => new TournamentDto(t.Id, t.Name, t.Venue, t.Date, t.EndDate))
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TournamentDto>> GetById(int id)
        {
            var t = await _db.Tournaments.FindAsync(id);
            return t is null ? NotFound() : Ok(new TournamentDto(t.Id, t.Name, t.Venue, t.Date, t.EndDate));
        }

        [HttpPost]
        public async Task<ActionResult<TournamentDto>> Create([FromBody] UpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var start = body.Date.Date;
            var end = body.EndDate?.Date;
            if (end.HasValue && end.Value < start)
                return BadRequest(new { error = "End date must be on or after start date" });

            var t = new Tournament { Name = body.Name.Trim(), Venue = body.Venue.Trim(), Date = start, EndDate = end };
            _db.Tournaments.Add(t);
            await _db.SaveChangesAsync();

            var dto = new TournamentDto(t.Id, t.Name, t.Venue, t.Date, t.EndDate);
            return CreatedAtAction(nameof(GetById), new { id = t.Id }, dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var t = await _db.Tournaments.FindAsync(id);
            if (t is null) return NotFound();

            var start = body.Date.Date;
            var end = body.EndDate?.Date;
            if (end.HasValue && end.Value < start)
                return BadRequest(new { error = "End date must be on or after start date" });

            t.Name = body.Name.Trim();
            t.Venue = body.Venue.Trim();
            t.Date = start;
            t.EndDate = end;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Tournaments.FindAsync(id);
            if (t is null) return NotFound();

            _db.Tournaments.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
