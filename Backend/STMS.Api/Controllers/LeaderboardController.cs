using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;

namespace STMS.Api.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly StmsDbContext _db;
        public LeaderboardController(StmsDbContext db)
        {
            _db = db;
        }

        // GET: api/leaderboard/{tournamentId}
        [HttpGet("{tournamentId}")]
        public async Task<IActionResult> GetLeaderboard(int tournamentId)
        {
            // Get all events for tournament
            var eventIds = await _db.TournamentEvents
                .Where(e => e.TournamentId == tournamentId)
                .Select(e => e.Id)
                .ToListAsync();

            // Get all timings for those events
            var timings = await _db.Timings
                .Where(t => eventIds.Contains(t.EventId))
                .ToListAsync();

            // Get player info (by university in tournament)
            var universityIds = await _db.Universities
                .Where(u => u.TournamentId == tournamentId)
                .Select(u => u.Id)
                .ToListAsync();

            var players = await _db.Players
                .Where(p => universityIds.Contains(p.UniversityId))
                .Include(p => p.University)
                .ToListAsync();

            // Aggregate points per player
            var playerPoints = new Dictionary<int, int>();
            foreach (var eventId in eventIds)
            {
                var eventTimings = timings.Where(t => t.EventId == eventId && t.TimeMs > 0)
                    .OrderBy(t => t.TimeMs)
                    .ToList();
                for (int i = 0; i < eventTimings.Count; i++)
                {
                    var rank = i + 1;
                    int points = rank switch
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
                    if (!playerPoints.ContainsKey(eventTimings[i].PlayerId))
                        playerPoints[eventTimings[i].PlayerId] = 0;
                    playerPoints[eventTimings[i].PlayerId] += points;
                }
            }

            // Build player leaderboard
            var playerLeaderboard = players
                .Where(p => playerPoints.ContainsKey(p.Id))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    university = p.University != null ? p.University.Name : "",
                    universityId = p.UniversityId,
                    totalPoints = playerPoints[p.Id]
                })
                .OrderByDescending(x => x.totalPoints)
                .ToList();

            // Aggregate points per university
            var universityPoints = new Dictionary<int, int>();
            foreach (var player in playerLeaderboard)
            {
                if (!universityPoints.ContainsKey(player.universityId))
                    universityPoints[player.universityId] = 0;
                universityPoints[player.universityId] += player.totalPoints;
            }

            // Build university leaderboard
            var universityLeaderboard = _db.Universities
                .Where(u => universityIds.Contains(u.Id))
                .ToList()
                .Where(u => universityPoints.ContainsKey(u.Id))
                .Select(u => new
                {
                    id = u.Id,
                    name = u.Name,
                    totalPoints = universityPoints[u.Id]
                })
                .OrderByDescending(x => x.totalPoints)
                .ToList();

            // Return both leaderboards
            return Ok(new {
                players = playerLeaderboard,
                universities = universityLeaderboard
            });
        }
    }
}
