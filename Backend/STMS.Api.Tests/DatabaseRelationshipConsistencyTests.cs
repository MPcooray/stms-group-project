using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using Xunit;

namespace STMS.Api.Tests
{
    public class DatabaseRelationshipConsistencyTests
    {
        [Fact]
        public async Task UpdateUniversity_UpdatesRelatedPlayers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "RelationshipUpdateTest")
                .Options;
            using var context = new StmsDbContext(options);

            var uni = new University { Id = 1, Name = "U1", TournamentId = 1 };
            var player = new Player { Id = 1, Name = "P1", UniversityId = 1 };
            context.Universities.Add(uni);
            context.Players.Add(player);
            await context.SaveChangesAsync();

            // Act: Update university name
            uni.Name = "U1 Updated";
            context.Universities.Update(uni);
            await context.SaveChangesAsync();

            // Assert: Player's UniversityId remains valid
            var updatedPlayer = context.Players.Include(p => p.University).FirstOrDefault(p => p.Id == 1);
            Assert.NotNull(updatedPlayer);
            Assert.Equal(1, updatedPlayer.UniversityId);
        }

        [Fact]
        public async Task DeleteUniversity_RemovesRelatedPlayers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "RelationshipDeleteTest")
                .Options;
            using var context = new StmsDbContext(options);

            var uni = new University { Id = 2, Name = "U2", TournamentId = 1 };
            var player = new Player { Id = 2, Name = "P2", UniversityId = 2 };
            context.Universities.Add(uni);
            context.Players.Add(player);
            await context.SaveChangesAsync();

            // Act: Delete university
            context.Universities.Remove(uni);
            await context.SaveChangesAsync();

            // Assert: Related player is also deleted
            var deletedPlayer = context.Players.FirstOrDefault(p => p.Id == 2);
            Assert.Null(deletedPlayer);
        }
    }
}
