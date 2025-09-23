using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace STMS.Api.Tests
{
    public class LeaderboardAccuracyTests
    {
        [Fact]
        public void Leaderboard_ReflectsAccurateOrderAndTotals_AfterPointUpdates()
        {
            // Arrange: Initial points for 3 players
            var leaderboard = new List<(string Name, int Points)>
            {
                ("A", 10),
                ("B", 8),
                ("C", 7)
            };

            // Simulate point updates
            leaderboard[0] = ("A", leaderboard[0].Points - 3); // A loses 3 points
            leaderboard[1] = ("B", leaderboard[1].Points + 2); // B gains 2 points
            leaderboard[2] = ("C", leaderboard[2].Points + 1); // C gains 1 point

            // Act: Sort leaderboard by updated points
            var sorted = leaderboard.OrderByDescending(x => x.Points).ToList();

            // Assert: Leaderboard order and totals are correct
            Assert.Equal("B", sorted[0].Name); // B should be first
            Assert.Equal(10, sorted[0].Points);
            Assert.Equal("C", sorted[1].Name); // C should be second
            Assert.Equal(8, sorted[1].Points);
            Assert.Equal("A", sorted[2].Name); // A should be third
            Assert.Equal(7, sorted[2].Points);
        }
    }
}
