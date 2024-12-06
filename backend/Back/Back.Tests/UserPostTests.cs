using Back.Models;
using Xunit;

namespace Back.Tests
{
    public class UserPostTests
    {
        [Fact]
        public void MapToUserPostDto_ValidUser_ReturnsCorrectUserPost()
        {
            // Arrange
            var user = new User(
                username: "testUser",
                email: "test@test.com",
                password: "password123",
                firstName: "John",
                lastName: "Doe",
                phoneNumber: "+48123456789",
                accessFee: 0,
                accountStatus: "active",
                accountLevel: "user",
                description: "Test description",
                profileImage: "test.jpg"
            );

            // Act
            var userPost = UserPost.MapToUserPostDto(user);

            // Assert
            Assert.NotNull(userPost);
            Assert.Equal(user.Username, userPost.Username);
            Assert.Equal(user.FirstName, userPost.FirstName);
            Assert.Equal(user.LastName, userPost.LastName);
        }
    }
}
