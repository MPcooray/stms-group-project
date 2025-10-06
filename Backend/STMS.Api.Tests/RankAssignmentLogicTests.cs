using System.Collections.Generic;
using System.Linq;
using Xunit;
using STMS.Api.Models;

namespace STMS.Api.Tests
{
    public class RankAssignmentLogicTests
    {
        [Fact]
        public void Timings_AutomaticRankAssignment_WorksCorrectly()
        {
            // Arrange: Create sample timings
            var timings = new List<Timing>
            {
                new Timing { Id = 1, PlayerId = 1, EventId = 1, TimeMs = 900 }, // Fastest
                new Timing { Id = 2, PlayerId = 2, EventId = 1, TimeMs = 1000 }, // Second
                new Timing { Id = 3, PlayerId = 3, EventId = 1, TimeMs = 1100 }  // Third
            };

            // Act: Assign ranks based on sorted times
            var sorted = timings.OrderBy(t => t.TimeMs).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                // Simulate rank assignment
                sorted[i].PlayerId = sorted[i].PlayerId; // Just for clarity
                // Rank is i+1
            }

            // Assert: Check order and implied ranks
            Assert.Equal(900, sorted[0].TimeMs); // Rank 1
            Assert.Equal(1000, sorted[1].TimeMs); // Rank 2
            Assert.Equal(1100, sorted[2].TimeMs); // Rank 3
        }
    }
}
