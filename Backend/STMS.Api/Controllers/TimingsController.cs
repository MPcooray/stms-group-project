using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STMS.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class TimingsController : ControllerBase
    {
        private readonly StmsDbContext _db;
        public TimingsController(StmsDbContext db) => _db = db;

        public record TimingDto(int Id, int PlayerId, int EventId, int TimeMs, System.DateTime CreatedAt);
        public class TimingUpsertDto
        {
            public int PlayerId { get; set; }
            public int EventId { get; set; }
            public int TimeMs { get; set; }
        }

        // GET /api/timings/{playerId}/{eventId}
        [HttpGet("timings/{playerId:int}/{eventId:int}")]
        public async Task<ActionResult<TimingDto>> GetTiming(int playerId, int eventId)
        {
            var timing = await _db.Timings.AsNoTracking().FirstOrDefaultAsync(x => x.PlayerId == playerId && x.EventId == eventId);
            if (timing == null) return NotFound();
            return Ok(new TimingDto(timing.Id, timing.PlayerId, timing.EventId, timing.TimeMs, timing.CreatedAt));
        }

        // POST /api/timings
        [HttpPost("timings")]
        public async Task<ActionResult<TimingDto>> CreateOrUpdateTiming([FromBody] TimingUpsertDto dto)
        {
            var timing = await _db.Timings.FirstOrDefaultAsync(x => x.PlayerId == dto.PlayerId && x.EventId == dto.EventId);
            if (timing == null)
            {
                timing = new Timing { PlayerId = dto.PlayerId, EventId = dto.EventId, TimeMs = dto.TimeMs };
                _db.Timings.Add(timing);
            }
            else
            {
                timing.TimeMs = dto.TimeMs;
            }
            await _db.SaveChangesAsync();
            return Ok(new TimingDto(timing.Id, timing.PlayerId, timing.EventId, timing.TimeMs, timing.CreatedAt));
        }
    }
}
