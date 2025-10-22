using Microsoft.EntityFrameworkCore;
using STMS.Api.Controllers;
using STMS.Api.Data;
using STMS.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;

namespace STMS.Api.Tests
{
    public class LeaderboardGenderFilterTests
    {
        private static JsonElement ToJson(object value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private StmsDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var db = new StmsDbContext(options);

            // Seed a tournament with universities, players, events, timings
            var t = new Tournament { Id = 1, Name = "Test", Venue = "V", Date = System.DateTime.UtcNow.Date };
            db.Tournaments.Add(t);
            var u1 = new University { Id = 1, TournamentId = 1, Name = "U1" };
            var u2 = new University { Id = 2, TournamentId = 1, Name = "U2" };
            db.Universities.AddRange(u1, u2);
            var p1 = new Player { Id = 1, Name = "M1", UniversityId = 1, Gender = "Male" };
            var p2 = new Player { Id = 2, Name = "F1", UniversityId = 1, Gender = "Female" };
            var p3 = new Player { Id = 3, Name = "M2", UniversityId = 2, Gender = "Male" };
            var p4 = new Player { Id = 4, Name = "F2", UniversityId = 2, Gender = "Female" };
            db.Players.AddRange(p1, p2, p3, p4);

            var e1 = new TournamentEvent { Id = 1, TournamentId = 1, Name = "100m" };
            var e2 = new TournamentEvent { Id = 2, TournamentId = 1, Name = "200m" };
            db.TournamentEvents.AddRange(e1, e2);

            // Timings: rank order M1 < M2 and F1 < F2
            db.Timings.AddRange(
                new Timing { Id = 1, PlayerId = 1, EventId = 1, TimeMs = 11000 },
                new Timing { Id = 2, PlayerId = 3, EventId = 1, TimeMs = 12000 },
                new Timing { Id = 3, PlayerId = 2, EventId = 1, TimeMs = 13000 },
                new Timing { Id = 4, PlayerId = 4, EventId = 1, TimeMs = 14000 },
                new Timing { Id = 5, PlayerId = 1, EventId = 2, TimeMs = 22000 },
                new Timing { Id = 6, PlayerId = 3, EventId = 2, TimeMs = 23000 },
                new Timing { Id = 7, PlayerId = 2, EventId = 2, TimeMs = 24000 },
                new Timing { Id = 8, PlayerId = 4, EventId = 2, TimeMs = 25000 }
            );

            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task Leaderboard_Filters_Male_Only()
        {
            using var db = CreateDb();
            var controller = new LeaderboardController(db);

            var result = await controller.GetLeaderboard(1, "male") as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(result);
            var root = ToJson(result!.Value!);
            var players = root.GetProperty("players").EnumerateArray().ToList();

            // Should include only male players M1 and M2
            Assert.All(players, p => Assert.Equal("Male", p.GetProperty("gender").GetString()));
            Assert.Contains(players, p => p.GetProperty("name").GetString() == "M1");
            Assert.Contains(players, p => p.GetProperty("name").GetString() == "M2");
            Assert.DoesNotContain(players, p => p.GetProperty("name").GetString() == "F1");
            Assert.DoesNotContain(players, p => p.GetProperty("name").GetString() == "F2");
        }

        [Fact]
        public async Task Leaderboard_Filters_Female_Only()
        {
            using var db = CreateDb();
            var controller = new LeaderboardController(db);

            var result = await controller.GetLeaderboard(1, "female") as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(result);
            var root = ToJson(result!.Value!);
            var players = root.GetProperty("players").EnumerateArray().ToList();

            // Should include only female players F1 and F2
            Assert.All(players, p => Assert.Equal("Female", p.GetProperty("gender").GetString()));
            Assert.Contains(players, p => p.GetProperty("name").GetString() == "F1");
            Assert.Contains(players, p => p.GetProperty("name").GetString() == "F2");
            Assert.DoesNotContain(players, p => p.GetProperty("name").GetString() == "M1");
            Assert.DoesNotContain(players, p => p.GetProperty("name").GetString() == "M2");
        }

        [Fact]
        public async Task Leaderboard_NoGender_Returns_All()
        {
            using var db = CreateDb();
            var controller = new LeaderboardController(db);

            var result = await controller.GetLeaderboard(1, null) as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(result);
            var root = ToJson(result!.Value!);
            var players = root.GetProperty("players").EnumerateArray().ToList();
            Assert.Equal(4, players.Count);
        }
    }
}
