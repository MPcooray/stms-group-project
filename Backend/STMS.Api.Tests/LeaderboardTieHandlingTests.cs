using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using STMS.Api.Controllers;
using STMS.Api.Data;
using STMS.Api.Models;

namespace STMS.Api.Tests
{
    public class LeaderboardTieHandlingTests
    {
        private StmsDbContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new StmsDbContext(options);
        }

        [Fact]
        public async void TwoWayTie_FirstPlace_AssignsSamePointsAndNextRankSkips()
        {
            using var db = CreateInMemoryDb("TwoWayTieFirst");

                // Arrange: Tournament, university, players, event and timings
                var tournament = new Tournament { Id = 1, Name = "TieCup" };
                var uni = new University { Id = 10, Name = "Uni A", TournamentId = 1 };
                var p1 = new Player { Id = 1, Name = "P1", UniversityId = 10, Gender = "Male" };
                var p2 = new Player { Id = 2, Name = "P2", UniversityId = 10, Gender = "Male" };
                var p3 = new Player { Id = 3, Name = "P3", UniversityId = 10, Gender = "Male" };

                var evt = new TournamentEvent { Id = 100, TournamentId = 1, Name = "100m" };

                // p1 and p2 tie for first (same time), p3 third
                var t1 = new Timing { Id = 1, PlayerId = 1, EventId = 100, TimeMs = 900 };
                var t2 = new Timing { Id = 2, PlayerId = 2, EventId = 100, TimeMs = 900 };
                var t3 = new Timing { Id = 3, PlayerId = 3, EventId = 100, TimeMs = 1100 };

                db.Tournaments.Add(tournament);
                db.Universities.Add(uni);
                db.Players.AddRange(p1, p2, p3);
                db.TournamentEvents.Add(evt);
                db.Timings.AddRange(t1, t2, t3);
                db.SaveChanges();

            var controller = new LeaderboardController(db);

            // Act
            var result = await controller.GetLeaderboard(1, null) as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(result);

            // Serialize the returned object and parse the JSON to access 'players' reliably
            var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var playersEl = doc.RootElement.GetProperty("players");
            var players = playersEl.EnumerateArray().Select(e => e).ToList();

            // Two players should share rank 1 and both have 10 points; next player should have rank 3 with 7 points
            int GetInt(System.Text.Json.JsonElement el, string prop) => el.GetProperty(prop).GetInt32();
            int p1rank = GetInt(players.First(e => e.GetProperty("id").GetInt32() == 1), "rank");
            int p1pts = GetInt(players.First(e => e.GetProperty("id").GetInt32() == 1), "totalPoints");
            int p2rank = GetInt(players.First(e => e.GetProperty("id").GetInt32() == 2), "rank");
            int p2pts = GetInt(players.First(e => e.GetProperty("id").GetInt32() == 2), "totalPoints");
            int p3rank = GetInt(players.First(e => e.GetProperty("id").GetInt32() == 3), "rank");
            int p3pts = GetInt(players.First(e => e.GetProperty("id").GetInt32() == 3), "totalPoints");

            Assert.Equal(1, p1rank);
            Assert.Equal(10, p1pts);
            Assert.Equal(1, p2rank);
            Assert.Equal(10, p2pts);
            Assert.Equal(3, p3rank);
            Assert.Equal(7, p3pts);
        }

        [Fact]
        public async void ThreeWayTie_AssignsSamePointsToAllAndNextRankSkipsTo4()
        {
            using var db = CreateInMemoryDb("ThreeWayTie");

                var tournament = new Tournament { Id = 2, Name = "ThreeWay" };
                var uni = new University { Id = 20, Name = "Uni B", TournamentId = 2 };
                var players = new[] {
                    new Player { Id = 11, Name = "A", UniversityId = 20 },
                    new Player { Id = 12, Name = "B", UniversityId = 20 },
                    new Player { Id = 13, Name = "C", UniversityId = 20 },
                    new Player { Id = 14, Name = "D", UniversityId = 20 }
                };
                var evt = new TournamentEvent { Id = 200, TournamentId = 2 };

                // Three-way tie for first, D is fourth
                db.Tournaments.Add(tournament);
                db.Universities.Add(uni);
                db.Players.AddRange(players);
                db.TournamentEvents.Add(evt);
                db.Timings.AddRange(
                    new Timing { Id = 11, PlayerId = 11, EventId = 200, TimeMs = 800 },
                    new Timing { Id = 12, PlayerId = 12, EventId = 200, TimeMs = 800 },
                    new Timing { Id = 13, PlayerId = 13, EventId = 200, TimeMs = 800 },
                    new Timing { Id = 14, PlayerId = 14, EventId = 200, TimeMs = 1000 }
                );
                db.SaveChanges();

            var controller = new LeaderboardController(db);
            var result = await controller.GetLeaderboard(2, null) as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(result);
            var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var pls = doc.RootElement.GetProperty("players").EnumerateArray().ToList();

            System.Text.Json.JsonElement GetById(int id) => pls.First(e => e.GetProperty("id").GetInt32() == id);

            var a = GetById(11);
            var b = GetById(12);
            var c = GetById(13);
            var d = GetById(14);

            Assert.Equal(1, a.GetProperty("rank").GetInt32());
            Assert.Equal(10, a.GetProperty("totalPoints").GetInt32());
            Assert.Equal(1, b.GetProperty("rank").GetInt32());
            Assert.Equal(10, b.GetProperty("totalPoints").GetInt32());
            Assert.Equal(1, c.GetProperty("rank").GetInt32());
            Assert.Equal(10, c.GetProperty("totalPoints").GetInt32());

            Assert.Equal(4, d.GetProperty("rank").GetInt32());
            Assert.Equal(5, d.GetProperty("totalPoints").GetInt32());
        }

        [Fact]
        public async void MultipleEvents_TiesAcrossEvents_AggregatePointsCorrectly()
        {
            using var db = CreateInMemoryDb("MultiEventTies");

                var tournament = new Tournament { Id = 3, Name = "MultiEvent" };
                var uni = new University { Id = 30, Name = "Uni C", TournamentId = 3 };
                var p1 = new Player { Id = 21, Name = "P1", UniversityId = 30 };
                var p2 = new Player { Id = 22, Name = "P2", UniversityId = 30 };
                var e1 = new TournamentEvent { Id = 301, TournamentId = 3 };
                var e2 = new TournamentEvent { Id = 302, TournamentId = 3 };

                // Event 1: tie for 1st between p1 & p2 -> both 10
                // Event 2: p1 first (10), p2 second (8)
                db.Tournaments.Add(tournament);
                db.Universities.Add(uni);
                db.Players.AddRange(p1, p2);
                db.TournamentEvents.AddRange(e1, e2);
                db.Timings.AddRange(
                    new Timing { Id = 21, PlayerId = 21, EventId = 301, TimeMs = 900 },
                    new Timing { Id = 22, PlayerId = 22, EventId = 301, TimeMs = 900 },
                    new Timing { Id = 23, PlayerId = 21, EventId = 302, TimeMs = 800 },
                    new Timing { Id = 24, PlayerId = 22, EventId = 302, TimeMs = 900 }
                );
                db.SaveChanges();

            var controller = new LeaderboardController(db);
            var result = await controller.GetLeaderboard(3, null) as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(result);
            var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var pls = doc.RootElement.GetProperty("players").EnumerateArray().ToList();
            var p1res = pls.First(e => e.GetProperty("id").GetInt32() == 21);
            var p2res = pls.First(e => e.GetProperty("id").GetInt32() == 22);

            Assert.Equal(20, p1res.GetProperty("totalPoints").GetInt32());
            Assert.Equal(18, p2res.GetProperty("totalPoints").GetInt32());
            Assert.True(p1res.GetProperty("rank").GetInt32() < p2res.GetProperty("rank").GetInt32());
        }

        [Fact]
        public async void TieAtBottom_NoPointsLost_AllPlayersPresent()
        {
            using var db = CreateInMemoryDb("TieAtBottom");

                var tournament = new Tournament { Id = 4, Name = "TieBottom" };
                var uni = new University { Id = 40, Name = "Uni D", TournamentId = 4 };
                var p1 = new Player { Id = 31, Name = "Top", UniversityId = 40 };
                var p2 = new Player { Id = 32, Name = "Mid", UniversityId = 40 };
                var p3 = new Player { Id = 33, Name = "BotA", UniversityId = 40 };
                var p4 = new Player { Id = 34, Name = "BotB", UniversityId = 40 };
                var evt = new TournamentEvent { Id = 401, TournamentId = 4 };

                // Top: 900 (1st)
                // Mid: 1000 (2nd)
                // BotA & BotB tie for 3rd
                db.Tournaments.Add(tournament);
                db.Universities.Add(uni);
                db.Players.AddRange(p1, p2, p3, p4);
                db.TournamentEvents.Add(evt);
                db.Timings.AddRange(
                    new Timing { Id = 31, PlayerId = 31, EventId = 401, TimeMs = 900 },
                    new Timing { Id = 32, PlayerId = 32, EventId = 401, TimeMs = 1000 },
                    new Timing { Id = 33, PlayerId = 33, EventId = 401, TimeMs = 1100 },
                    new Timing { Id = 34, PlayerId = 34, EventId = 401, TimeMs = 1100 }
                );
                db.SaveChanges();

            var controller = new LeaderboardController(db);
            var result = await controller.GetLeaderboard(4, null) as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(result);
            var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var pls = doc.RootElement.GetProperty("players").EnumerateArray().ToList();

            System.Text.Json.JsonElement GetById(int id) => pls.First(e => e.GetProperty("id").GetInt32() == id);
            var top = GetById(31);
            var mid = GetById(32);
            var botA = GetById(33);
            var botB = GetById(34);

            Assert.Equal(3, botA.GetProperty("rank").GetInt32());
            Assert.Equal(7, botA.GetProperty("totalPoints").GetInt32());
            Assert.Equal(3, botB.GetProperty("rank").GetInt32());
            Assert.Equal(7, botB.GetProperty("totalPoints").GetInt32());

            int sum = pls.Sum(e => e.GetProperty("totalPoints").GetInt32());
            Assert.Equal(32, sum);
        }
    }
}
