using Microsoft.EntityFrameworkCore;
using STMS.Api.Controllers;
using STMS.Api.Data;
using STMS.Api.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;

namespace STMS.Api.Tests
{
    public class EventResultsGenderFilterTests
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

            // Seed tournament and event
            var t = new Tournament { Id = 1, Name = "T1", Venue = "V", Date = System.DateTime.UtcNow.Date };
            db.Tournaments.Add(t);
            var u = new University { Id = 1, Name = "U1", TournamentId = 1 };
            db.Universities.Add(u);
            var e = new TournamentEvent { Id = 10, TournamentId = 1, Name = "100m" };
            db.TournamentEvents.Add(e);
            // Players with mixed genders and times
            var m1 = new Player { Id = 1, Name = "M1", UniversityId = 1, Gender = "Male" };
            var f1 = new Player { Id = 2, Name = "F1", UniversityId = 1, Gender = "Female" };
            var m2 = new Player { Id = 3, Name = "M2", UniversityId = 1, Gender = "male" }; // lower-case to test case-insensitive
            db.Players.AddRange(m1, f1, m2);
            db.Timings.AddRange(
                new Timing { Id = 1, EventId = 10, PlayerId = 1, TimeMs = 11000 },
                new Timing { Id = 2, EventId = 10, PlayerId = 2, TimeMs = 13000 },
                new Timing { Id = 3, EventId = 10, PlayerId = 3, TimeMs = 12000 }
            );
            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task EventResults_Filters_Male()
        {
            using var db = CreateDb();
            var controller = new TournamentEventsController(db);
            var result = await controller.GetEventResults(10, "male");
            Assert.NotNull(result);
            var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var root = ToJson(ok.Value!);
            var arr = root.EnumerateArray().ToList();
            Assert.Equal(2, arr.Count); // M1 and M2 only
            Assert.Contains(arr, x => x.GetProperty("playerName").GetString() == "M1");
            Assert.Contains(arr, x => x.GetProperty("playerName").GetString() == "M2");
        }

        [Fact]
        public async Task EventResults_Filters_Female()
        {
            using var db = CreateDb();
            var controller = new TournamentEventsController(db);
            var result = await controller.GetEventResults(10, "female");
            Assert.NotNull(result);
            var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var root = ToJson(ok.Value!);
            var arr = root.EnumerateArray().ToList();
            Assert.Single(arr);
            Assert.Equal("F1", arr[0].GetProperty("playerName").GetString());
        }

        [Fact]
        public async Task EventResults_NoFilter_All()
        {
            using var db = CreateDb();
            var controller = new TournamentEventsController(db);
            var result = await controller.GetEventResults(10, null);
            Assert.NotNull(result);
            var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var root = ToJson(ok.Value!);
            var arr = root.EnumerateArray().ToList();
            Assert.Equal(3, arr.Count);
        }
    }
}
