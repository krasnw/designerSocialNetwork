using Back.Controllers;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Back.Models;

namespace Back.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new AuthController(_authServiceMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginRequest = new User.LoginRequest 
            { 
                Username = "testUser", 
                Password = "password123" 
            };
            var expectedToken = "test.jwt.token";

            _userServiceMock.Setup(x => x.Login(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(true);
            _authServiceMock.Setup(x => x.GenerateToken(loginRequest.Username))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            Assert.Equal(expectedToken, value.token.ToString());
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new User.LoginRequest 
            { 
                Username = "wrongUser", 
                Password = "wrongPass" 
            };

            _userServiceMock.Setup(x => x.Login(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            dynamic value = unauthorizedResult.Value;
            Assert.Equal("Invalid username or password", value.message.ToString());
        }

        [Fact]
        public async Task SignUp_ValidData_ReturnsOkWithToken()
        {
            // Arrange
            var signUpRequest = new User.SignUpRequest
            {
                Username = "newUser",
                Email = "new@user.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+48123456789"
            };
            var expectedToken = "new.user.token";

            _userServiceMock.Setup(x => x.SignUp(
                signUpRequest.Username,
                signUpRequest.Email,
                signUpRequest.Password,
                signUpRequest.FirstName,
                signUpRequest.LastName,
                signUpRequest.PhoneNumber,
                It.IsAny<string>()))
                .Returns(string.Empty);

            _authServiceMock.Setup(x => x.GenerateToken(signUpRequest.Username))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _controller.SignUp(signUpRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            Assert.Equal(expectedToken, value.token.ToString());
        }

        [Fact]
        public async Task SignUp_DuplicateUsername_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new User.SignUpRequest
            {
                Username = "existingUser",
                Email = "existing@user.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+48123456789"
            };
            var errorMessage = "Username already exists";

            _userServiceMock.Setup(x => x.SignUp(
                signUpRequest.Username,
                signUpRequest.Email,
                signUpRequest.Password,
                signUpRequest.FirstName,
                signUpRequest.LastName,
                signUpRequest.PhoneNumber,
                It.IsAny<string>()))
                // Changed: return string directly instead of Task.FromResult.
                .Returns(errorMessage);

            // Act
            var result = await _controller.SignUp(signUpRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic value = badRequestResult.Value;
            Assert.Equal(errorMessage, value.message.ToString());
        }

        [Fact]
        public async Task SignUp_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new User.SignUpRequest
            {
                Username = "",  // Invalid empty username
                Email = "invalid@email",
                Password = "pass",
                FirstName = "",  // Invalid empty first name
                LastName = "Doe",
                PhoneNumber = "+48123456789"
            };

            _userServiceMock.Setup(x => x.SignUp(
                signUpRequest.Username,
                signUpRequest.Email,
                signUpRequest.Password,
                signUpRequest.FirstName,
                signUpRequest.LastName,
                signUpRequest.PhoneNumber,
                It.IsAny<string>()))
                // Changed: throw exception synchronously.
                .Throws(new ArgumentException("Invalid data"));

            // Act
            var result = await _controller.SignUp(signUpRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic value = badRequestResult.Value;
            Assert.Equal("Invalid data", value.message.ToString());
        }

        [Fact]
        public async Task RenewToken_ValidToken_ReturnsNewToken()
        {
            // Arrange
            var oldToken = "old.jwt.token";
            var newToken = "new.jwt.token";

            _authServiceMock.Setup(x => x.RenewToken(oldToken))
                // Changed: replace ReturnsAsync with Task.FromResult.
                .Returns(Task.FromResult(newToken));

            // Act
            var result = await _controller.RenewToken(oldToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            Assert.Equal(newToken, value.token.ToString());
        }
    }
}
