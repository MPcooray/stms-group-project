// using Xunit;
// using STMS.Api.Models;
// using System;

// namespace STMS.Api.Tests
// {
//     public class PlayerModelTests
//     {
//         [Fact]
//         public void Player_InitializesWithDefaultValues()
//         {
//             // Arrange
//             var player = new Player();

//             // Assert
//             Assert.Equal(0, player.Id);
//             Assert.Equal("", player.Name);
//             Assert.Null(player.University);
//             Assert.Null(player.Age);
//             Assert.Null(player.Gender);
//             Assert.True((DateTime.UtcNow - player.CreatedAt).TotalSeconds < 5);
//         }
//     }
// }
