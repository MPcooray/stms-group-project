using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace STMS.Api.Tests
{
    public class LeaderboardFilteringTests
    {
        [Fact]
        public void FilterLeaderboard_ByCategory_ReturnsCorrectPlayers()
        {
            // Arrange: Simulate leaderboard with categories
            var leaderboard = new List<(string Name, string Category, int Points)>
            {
                ("A", "Sprint", 10),
                ("B", "Sprint", 8),
                ("C", "Relay", 9),
                ("D", "Relay", 7)
            };

            // Act: Filter by category "Sprint"
            var sprintPlayers = leaderboard.Where(x => x.Category == "Sprint").ToList();

            // Assert
            Assert.Equal(2, sprintPlayers.Count);
            Assert.Contains(sprintPlayers, p => p.Name == "A");
            Assert.Contains(sprintPlayers, p => p.Name == "B");
        }

        [Fact]
        public void FilterLeaderboard_ByPoints_ReturnsCorrectPlayers()
        {
            // Arrange
            var leaderboard = new List<(string Name, int Points)>
            {
                ("A", 10),
                ("B", 8),
                ("C", 9)
            };

            // Act: Filter players with points >= 9
            var topPlayers = leaderboard.Where(x => x.Points >= 9).ToList();

            // Assert
            Assert.Equal(2, topPlayers.Count);
            Assert.Contains(topPlayers, p => p.Name == "A");
            Assert.Contains(topPlayers, p => p.Name == "C");
        }
    }
}
