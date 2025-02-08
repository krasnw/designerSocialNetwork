using Back.Models;
using Back.Models.UserDto;
using Back.Models.PostDto;
using Xunit;

namespace Back.Tests
{
    public class UserMiniDtoTests
    {
        [Fact]
        public void MapFromUser_ValidUser_ReturnsCorrectUserMiniDto()
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
            var userMiniDto = UserMiniDto.MapFromUser(user);

            // Assert
            Assert.NotNull(userMiniDto);
            Assert.Equal(user.Username, userMiniDto.Username);
            Assert.Equal(user.FirstName, userMiniDto.FirstName);
            Assert.Equal(user.LastName, userMiniDto.LastName);
            Assert.Equal(user.ProfileImage, userMiniDto.ProfileImage);  // Changed from ProfilePicture to ProfileImage
        }
    }
}
