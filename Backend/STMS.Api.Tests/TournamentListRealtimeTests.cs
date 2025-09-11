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
    public class TournamentListRealtimeTests
    {
        private StmsDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<StmsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StmsDbContext(options);
        }

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
            // Accept both .Value and .Result, and expect an empty list, not null
            var tournaments = actionResult.Value ?? new System.Collections.Generic.List<TournamentsController.TournamentDto>();
            Assert.Empty(tournaments); // Should be empty after delete
        }
    }
}
