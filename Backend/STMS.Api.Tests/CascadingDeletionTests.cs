using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using Xunit;

namespace STMS.Api.Tests
{
    public class CascadingDeletionTests
    {
        [Fact]
        public async Task DeletingUniversity_DeletesAssociatedPlayers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "CascadeDeleteTest1")
                .Options;
            using var context = new StmsDbContext(options);

            var uni = new University { Id = 1, Name = "U1", TournamentId = 1 };
            var player = new Player { Id = 1, Name = "A", UniversityId = 1 };
            context.Universities.Add(uni);
            context.Players.Add(player);
            await context.SaveChangesAsync();

            // Act
            context.Universities.Remove(uni);
            await context.SaveChangesAsync();

            // Assert
            Assert.Empty(context.Players.ToList());
        }

        [Fact]
        public async Task DeletingTournament_DeletesAssociatedEvents()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "CascadeDeleteTest2")
                .Options;
            using var context = new StmsDbContext(options);

            var tournament = new Tournament { Id = 1, Name = "T1" };
            var evt = new TournamentEvent { Id = 1, TournamentId = 1 };
            context.Tournaments.Add(tournament);
            context.TournamentEvents.Add(evt);
            await context.SaveChangesAsync();

            // Act
            context.Tournaments.Remove(tournament);
            await context.SaveChangesAsync();

            // Assert
            Assert.Empty(context.TournamentEvents.ToList());
        }
    }
}
