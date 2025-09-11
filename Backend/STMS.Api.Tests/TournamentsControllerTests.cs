using Xunit;
using STMS.Api.Controllers;
using STMS.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using STMS.Api.Data;
using Microsoft.EntityFrameworkCore.InMemory;
namespace STMS.Api.Tests
{
    public class TournamentsControllerTests
    {
        [Fact]
        public async Task Tournament_List_Reflects_Changes()
        {
            var db = GetDbContext();
            var controller = new TournamentsController(db);

            // Create
            var body = new TournamentsController.UpsertDto
            {
                Name = "Tournament 1",
                Venue = "Venue 1",
                Date = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };
            await controller.Create(body);

            // Update
            var tournament = db.Tournaments.First();
            var updateBody = new TournamentsController.UpsertDto
            {
                Name = "Tournament 1 Updated",
                Venue = "Venue 1 Updated",
                Date = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2)
            };
            await controller.Update(tournament.Id, updateBody);

            // Delete
            await controller.Delete(tournament.Id);

            // Get list
            var result = await controller.GetAll();
            var actionResult = Assert.IsType<ActionResult<System.Collections.Generic.IEnumerable<TournamentsController.TournamentDto>>>(result);
                var tournaments = actionResult.Value ?? new System.Collections.Generic.List<TournamentsController.TournamentDto>();
                Assert.Empty(tournaments); // Should be empty after delete
        }
        private StmsDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StmsDbContext(options);
        }

        [Fact]
        public async Task Admin_Can_Create_Tournament()
        {
            var db = GetDbContext();
            var controller = new TournamentsController(db);

            var body = new TournamentsController.UpsertDto
            {
                Name = "Test Tournament",
                Venue = "Test Venue",
                Date = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            var result = await controller.Create(body);
            var created = db.Tournaments.FirstOrDefault(t => t.Name == "Test Tournament");

            Assert.NotNull(created);
            Assert.Equal("Test Venue", created.Venue);
        }

        [Fact]
        public async Task Admin_Can_Update_Tournament()
        {
            var db = GetDbContext();
            var controller = new TournamentsController(db);
            var tournament = new Tournament { Name = "Old Name", Venue = "Old Venue", Date = DateTime.Today };
            db.Tournaments.Add(tournament);
            await db.SaveChangesAsync();

            var body = new TournamentsController.UpsertDto
            {
                Name = "Updated Name",
                Venue = "Updated Venue",
                Date = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2)
            };

            var result = await controller.Update(tournament.Id, body);
            var updated = db.Tournaments.Find(tournament.Id);

            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal("Updated Venue", updated.Venue);
            Assert.Equal(DateTime.Today.AddDays(2), updated.EndDate);
        }

        [Fact]
        public async Task Admin_Can_Delete_Tournament()
        {
            var db = GetDbContext();
            var controller = new TournamentsController(db);
            var tournament = new Tournament { Name = "Delete Me", Venue = "Venue", Date = DateTime.Today };
            db.Tournaments.Add(tournament);
            await db.SaveChangesAsync();

            var result = await controller.Delete(tournament.Id);
            var deleted = db.Tournaments.Find(tournament.Id);

            Assert.Null(deleted);
        }
    }
}
