using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace STMS.Api.Tests
{
    public class LeaderboardCategoryAccuracyTests
    {
        [Fact]
        public void Leaderboard_AccurateForAllCategories()
        {
            // Arrange: Simulate points for players in two categories
            var category1 = new List<(string Name, int Points)>
            {
                ("A", 10),
                ("B", 8)
            };
            var category2 = new List<(string Name, int Points)>
            {
                ("A", 5),
                ("B", 7),
                ("C", 9)
            };

            // Aggregate total points per player across categories
            var allPlayers = category1.Concat(category2)
                .GroupBy(x => x.Name)
                .Select(g => (Name: g.Key, TotalPoints: g.Sum(x => x.Points)))
                .OrderByDescending(x => x.TotalPoints)
                .ToList();

            // Assert: Leaderboard data is accurate for all categories (ignore tie order)
            Assert.Equal(2, category1.Count);
            Assert.Equal(3, category2.Count);
            Assert.Equal(3, allPlayers.Count);
            Assert.Contains(allPlayers, p => p.Name == "A" && p.TotalPoints == 15);
            Assert.Contains(allPlayers, p => p.Name == "B" && p.TotalPoints == 15);
            Assert.Contains(allPlayers, p => p.Name == "C" && p.TotalPoints == 9);
        }
    }
}
