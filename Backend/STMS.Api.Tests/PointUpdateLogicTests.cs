using System.Collections.Generic;
using System.Linq;
using Xunit;
using STMS.Api.Models;

namespace STMS.Api.Tests
{
    public class PointUpdateLogicTests
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
        public void SingleRankChange_UpdatesPointsCorrectly()
        {
            // Arrange: Player moves from rank 2 to rank 1
            int oldRank = 2;
            int newRank = 1;
            int oldPoints = GetPointsForRank(oldRank);
            int newPoints = GetPointsForRank(newRank);

            // Act: Update points
            int pointDelta = newPoints - oldPoints;

            // Assert
            Assert.Equal(2, pointDelta); // 10 - 8 = 2
        }

        [Fact]
        public void MultipleRankChanges_UpdatesPointsCorrectly()
        {
            // Arrange: 3 players, ranks change after event
            var oldRanks = new[] { 1, 2, 3 };
            var newRanks = new[] { 2, 1, 3 };
            var oldPoints = oldRanks.Select(GetPointsForRank).ToArray();
            var newPoints = newRanks.Select(GetPointsForRank).ToArray();

            // Act: Calculate point deltas
            var pointDeltas = new int[3];
            for (int i = 0; i < 3; i++)
            {
                pointDeltas[i] = newPoints[i] - oldPoints[i];
            }

            // Assert
            Assert.Equal(-2, pointDeltas[0]); // Player 1: 8 - 10
            Assert.Equal(2, pointDeltas[1]);  // Player 2: 10 - 8
            Assert.Equal(0, pointDeltas[2]);  // Player 3: 7 - 7
        }
    }
}
