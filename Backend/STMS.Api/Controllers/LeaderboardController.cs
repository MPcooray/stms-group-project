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
                
                // Handle tie cases in point allocation
                int eventRank = 1;
                for (int i = 0; i < eventTimings.Count; i++)
                {
                    // Determine actual rank (considering ties)
                    int rank = eventRank;
                    
                    // Count how many players have the same time (tie group)
                    int tieCount = 1;
                    while (i + tieCount < eventTimings.Count && 
                           eventTimings[i].TimeMs == eventTimings[i + tieCount].TimeMs)
                    {
                        tieCount++;
                    }
                    
                    // Get points for the current rank
                    int points = GetPointsForRank(rank);
                    
                    // Allocate points to all tied players
                    for (int j = 0; j < tieCount; j++)
                    {
                        if (!playerPoints.ContainsKey(eventTimings[i + j].PlayerId))
                            playerPoints[eventTimings[i + j].PlayerId] = 0;
                        playerPoints[eventTimings[i + j].PlayerId] += points;
                    }
                    
                    // Skip ahead by the number of tied players
                    i += tieCount - 1;
                    
                    // Next rank skips the tied positions
                    eventRank += tieCount;
                }
            }

            // Build player leaderboard with ranking
            var playerLeaderboardData = players
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

            // Assign ranks with tie handling
            var playerLeaderboard = new List<object>();
            int currentRank = 1;
            for (int i = 0; i < playerLeaderboardData.Count; i++)
            {
                int rank = currentRank;
                
                // Count how many players have the same points (tie group)
                int tieCount = 1;
                while (i + tieCount < playerLeaderboardData.Count && 
                       playerLeaderboardData[i].totalPoints == playerLeaderboardData[i + tieCount].totalPoints)
                {
                    tieCount++;
                }
                
                // Add all tied players with the same rank
                for (int j = 0; j < tieCount; j++)
                {
                    var player = playerLeaderboardData[i + j];
                    playerLeaderboard.Add(new
                    {
                        rank = rank,
                        id = player.id,
                        name = player.name,
                        university = player.university,
                        universityId = player.universityId,
                        totalPoints = player.totalPoints
                    });
                }
                
                // Skip ahead by the number of tied players
                i += tieCount - 1;
                
                // Next rank skips the tied positions
                currentRank += tieCount;
            }

            // Aggregate points per university
            var universityPoints = new Dictionary<int, int>();
            foreach (var player in playerLeaderboardData)
            {
                if (!universityPoints.ContainsKey(player.universityId))
                    universityPoints[player.universityId] = 0;
                universityPoints[player.universityId] += player.totalPoints;
            }

            // Build university leaderboard with ranking
            var universityLeaderboardData = _db.Universities
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

            // Assign ranks with tie handling for universities
            var universityLeaderboard = new List<object>();
            int universityRank = 1;
            for (int i = 0; i < universityLeaderboardData.Count; i++)
            {
                int rank = universityRank;
                
                // Count how many universities have the same points (tie group)
                int tieCount = 1;
                while (i + tieCount < universityLeaderboardData.Count && 
                       universityLeaderboardData[i].totalPoints == universityLeaderboardData[i + tieCount].totalPoints)
                {
                    tieCount++;
                }
                
                // Add all tied universities with the same rank
                for (int j = 0; j < tieCount; j++)
                {
                    var university = universityLeaderboardData[i + j];
                    universityLeaderboard.Add(new
                    {
                        rank = rank,
                        id = university.id,
                        name = university.name,
                        totalPoints = university.totalPoints
                    });
                }
                
                // Skip ahead by the number of tied universities
                i += tieCount - 1;
                
                // Next rank skips the tied positions
                universityRank += tieCount;
            }

            // Return both leaderboards
            return Ok(new {
                players = playerLeaderboard,
                universities = universityLeaderboard
            });
        }

        /// <summary>
        /// Returns points based on rank position
        /// </summary>
        private int GetPointsForRank(int rank)
        {
            return rank switch
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
    }
}
