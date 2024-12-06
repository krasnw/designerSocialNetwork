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
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserController(_userServiceMock.Object);
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
                new Dictionary<string, int>(),
                "Test description",
                "test.jpg",
                100
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
        public void EditMyProfile_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var editRequest = new User.EditRequest
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "+48123456789"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.EditMyProfile(editRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Blame the token, relog please", unauthorizedResult.Value);
        }
    }
}
