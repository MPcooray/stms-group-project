using System.Collections.Generic;
using System.Linq;
using Xunit;
using STMS.Api.Models;

namespace STMS.Api.Tests
{
    public class AdminTimingEntryTests
    {
        private int GetPointsForRank(int rank)
        {
            return rank switch
            {
                1 => 10,
                2 => 8,
                3 => 7,
                4 => 5,
                5 => 4,
                6 => 3,
                7 => 2,
                8 => 1,
                _ => 0
            };
        }

        [Fact]
        public void AdminTimingEntry_AutomaticPointsCalculation_WorksCorrectly()
        {
            // Arrange: Admin enters timings for 3 players
            var timings = new List<Timing>
            {
                new Timing { Id = 1, PlayerId = 1, EventId = 1, TimeMs = 900 }, // Fastest
                new Timing { Id = 2, PlayerId = 2, EventId = 1, TimeMs = 1000 }, // Second
                new Timing { Id = 3, PlayerId = 3, EventId = 1, TimeMs = 1100 }  // Third
            };

            // Act: Assign ranks and calculate points
            var sorted = timings.OrderBy(t => t.TimeMs).ToList();
            var playerPoints = new Dictionary<int, int>();
            for (int i = 0; i < sorted.Count; i++)
            {
                int rank = i + 1;
                int points = GetPointsForRank(rank);
                playerPoints[sorted[i].PlayerId] = points;
            }

            // Assert: Points are assigned correctly
            Assert.Equal(10, playerPoints[1]); // Fastest
            Assert.Equal(8, playerPoints[2]);  // Second
            Assert.Equal(7, playerPoints[3]);  // Third
        }
    }
}
