using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("tournaments")]
[Authorize]
public class TournamentsController : ControllerBase
{
    private readonly StmsDbContext _db;
    public TournamentsController(StmsDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IEnumerable<Tournament>> Get() =>
        await _db.Tournaments.AsNoTracking().OrderBy(t => t.Date).ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Tournament t)
    {
        if (string.IsNullOrWhiteSpace(t.Name)) return BadRequest("Name is required");
        _db.Tournaments.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Tournament dto)
    {
        var t = await _db.Tournaments.FindAsync(id);
        if (t is null) return NotFound();
        t.Name = dto.Name; t.Date = dto.Date; t.Venue = dto.Venue;
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
