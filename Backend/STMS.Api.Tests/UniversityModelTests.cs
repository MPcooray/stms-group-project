using Xunit;
using STMS.Api.Models;
using System;

namespace STMS.Api.Tests
{
    public class UniversityModelTests
    {
        [Fact]
        public void University_InitializesWithDefaultValues()
        {
            // Arrange
            var university = new University();

            // Assert
            Assert.Equal(0, university.Id);
            Assert.Equal("", university.Name);
            Assert.Equal(0, university.TournamentId);
            Assert.Null(university.Tournament);
            Assert.True((DateTime.UtcNow - university.CreatedAt).TotalSeconds < 5);
        }
    }
}
