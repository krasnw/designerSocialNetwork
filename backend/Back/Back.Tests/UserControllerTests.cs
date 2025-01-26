using Back.Controllers;
using Back.Models;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Back.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<IRatingService> _ratingServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _ratingServiceMock = new Mock<IRatingService>();
            _controller = new UserController(
                _userServiceMock.Object, 
                _subscriptionServiceMock.Object,
                _ratingServiceMock.Object);
        }

        [Fact]
        public void GetMyProfile_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            var testUsername = "testUser";
            var expectedProfile = new UserProfile(
                testUsername,
                "Test",
                "User",
                "Test description",
                "test.jpg",
                100,  // rubies
                0,    // totalLikes
                0     // completedTasks
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, testUsername)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _userServiceMock.Setup(x => x.GetOwnProfile(testUsername))
                .Returns(expectedProfile);

            // Act
            var result = _controller.GetMyProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProfile = Assert.IsType<UserProfile>(okResult.Value);
            Assert.Equal(testUsername, returnedProfile.Username);
        }

        [Fact]
        public async Task EditBasicInfo_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var editRequest = new User.EditBasicRequest
            {
                FirstName = "Test",
                LastName = "User",
                Description = "Test description"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.EditBasicInfo(editRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Blame the token, relog please", unauthorizedResult.Value);
        }

        [Fact]
        public async Task EditSensitiveInfo_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var editRequest = new User.EditSensitiveRequest
            {
                CurrentPassword = "oldPassword",
                NewEmail = "test@test.com",
                NewPhoneNumber = "+48123456789"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.EditSensitiveInfo(editRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Blame the token, relog please", unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetUser_ValidUsername_ReturnsOkResult()
        {
            // Arrange
            var testUsername = "testUser";
            var expectedProfile = new UserProfile(
                testUsername,
                "Test",
                "User",
                "Test description",
                "test.jpg",
                100,  // rubies
                0,    // totalLikes
                0     // completedTasks
            );

            _userServiceMock.Setup(x => x.GetProfile(testUsername))
                .Returns(expectedProfile);
            _subscriptionServiceMock.Setup(x => x.IsSubscribed(It.IsAny<string>(), testUsername))
                .ReturnsAsync(false);
            _ratingServiceMock.Setup(x => x.GetRatingPosition(testUsername))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.GetUser(testUsername);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic returnedProfile = okResult.Value;
            Assert.Equal(testUsername, returnedProfile.Username);
            Assert.Equal(1, returnedProfile.RatingPosition);
        }
    }
}
