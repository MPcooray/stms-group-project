using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using Xunit;

namespace STMS.Api.Tests
{
    public class RankDatabaseOrderTests
    {
        [Fact]
        public async Task Timings_ImpliedRankOrder_IsCorrectInDatabase()
        {
            // Arrange: Setup in-memory DB
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "RankOrderDbTest")
                .Options;
            using var context = new StmsDbContext(options);

            // Add university
            var uni = new University { Id = 1, Name = "U1", TournamentId = 1 };
            context.Universities.Add(uni);
            // Add players
            var p1 = new Player { Id = 1, Name = "A", UniversityId = 1 };
            var p2 = new Player { Id = 2, Name = "B", UniversityId = 1 };
            var p3 = new Player { Id = 3, Name = "C", UniversityId = 1 };
            context.Players.AddRange(p1, p2, p3);
            // Add event
            var evt = new TournamentEvent { Id = 1, TournamentId = 1 };
            context.TournamentEvents.Add(evt);
            // Add timings
            context.Timings.AddRange(
                new Timing { Id = 1, EventId = 1, PlayerId = 1, TimeMs = 900 }, // Fastest
                new Timing { Id = 2, EventId = 1, PlayerId = 2, TimeMs = 1000 }, // Second
                new Timing { Id = 3, EventId = 1, PlayerId = 3, TimeMs = 1100 }  // Third
            );
            await context.SaveChangesAsync();

            // Act: Retrieve timings ordered by time
            var storedTimings = context.Timings.Where(t => t.EventId == 1).OrderBy(t => t.TimeMs).ToList();

            // Assert: Implied rank by order
            Assert.Equal(3, storedTimings.Count);
            Assert.Equal(900, storedTimings[0].TimeMs); // Implied rank 1
            Assert.Equal(1000, storedTimings[1].TimeMs); // Implied rank 2
            Assert.Equal(1100, storedTimings[2].TimeMs); // Implied rank 3
        }
    }
}
