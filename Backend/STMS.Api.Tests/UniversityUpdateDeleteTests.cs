using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using STMS.Api.Data;
using STMS.Api.Models;
using Xunit;

namespace STMS.Api.Tests
{
    public class UniversityUpdateDeleteTests
    {
        [Fact]
        public async Task UpdateUniversity_ChangesDetailsCorrectly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "UniversityUpdateTest")
                .Options;
            using var context = new StmsDbContext(options);

            var uni = new University { Id = 1, Name = "U1", TournamentId = 1 };
            context.Universities.Add(uni);
            await context.SaveChangesAsync();

            // Act: Update university name
            uni.Name = "U1 Updated";
            context.Universities.Update(uni);
            await context.SaveChangesAsync();

            // Assert
            var updatedUni = context.Universities.FirstOrDefault(u => u.Id == 1);
            Assert.NotNull(updatedUni);
            Assert.Equal("U1 Updated", updatedUni.Name);
        }

        [Fact]
        public async Task DeleteUniversity_RemovesItFromDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(databaseName: "UniversityDeleteTest")
                .Options;
            using var context = new StmsDbContext(options);

            var uni = new University { Id = 2, Name = "U2", TournamentId = 1 };
            context.Universities.Add(uni);
            await context.SaveChangesAsync();

            // Act: Delete university
            context.Universities.Remove(uni);
            await context.SaveChangesAsync();

            // Assert
            var deletedUni = context.Universities.FirstOrDefault(u => u.Id == 2);
            Assert.Null(deletedUni);
        }
    }
}
