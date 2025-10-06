using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using Xunit;

namespace STMS.Api.Tests
{
    public class PlayerUpdateDeleteTests
    {
        [Fact]
        public async Task UpdatePlayer_ChangesDetailsCorrectly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "PlayerUpdateTest")
                .Options;
            using var context = new StmsDbContext(options);

            var player = new Player { Id = 1, Name = "P1", UniversityId = 1 };
            context.Players.Add(player);
            await context.SaveChangesAsync();

            // Act: Update player name
            player.Name = "P1 Updated";
            context.Players.Update(player);
            await context.SaveChangesAsync();

            // Assert
            var updatedPlayer = context.Players.FirstOrDefault(p => p.Id == 1);
            Assert.NotNull(updatedPlayer);
            Assert.Equal("P1 Updated", updatedPlayer.Name);
        }

        [Fact]
        public async Task DeletePlayer_RemovesItFromDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "PlayerDeleteTest")
                .Options;
            using var context = new StmsDbContext(options);

            var player = new Player { Id = 2, Name = "P2", UniversityId = 1 };
            context.Players.Add(player);
            await context.SaveChangesAsync();

            // Act: Delete player
            context.Players.Remove(player);
            await context.SaveChangesAsync();

            // Assert
            var deletedPlayer = context.Players.FirstOrDefault(p => p.Id == 2);
            Assert.Null(deletedPlayer);
        }
    }
}
