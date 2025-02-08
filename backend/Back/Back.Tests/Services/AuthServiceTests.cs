using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Moq;
using Xunit;

namespace Back.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly string originalSecretKey;
        private readonly string originalIssuer;
        private readonly string originalAudience;
        private readonly Mock<IDatabaseService> _databaseServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Store original environment variables
            originalSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "";
            originalIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "";
            originalAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "";

            // Set test environment variables
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "your-256-bit-secret-your-256-bit-secret-your-256-bit-secret");
            Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");

            _databaseServiceMock = new Mock<IDatabaseService>();
            _databaseServiceMock.Setup(x => x.GetUserStatus(It.IsAny<string>()))
                .ReturnsAsync("active");
            
            _authService = new AuthService(_databaseServiceMock.Object);
        }

        public void Dispose()
        {
            // Restore original environment variables
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", originalSecretKey);
            Environment.SetEnvironmentVariable("JWT_ISSUER", originalIssuer);
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", originalAudience);
        }

        [Fact]
        public async Task GenerateToken_ValidUsername_ReturnsToken()
        {
            // Arrange
            var username = "testUser";

            // Act
            var token = await _authService.GenerateToken(username);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task GenerateToken_FrozenUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var username = "frozenUser";
            _databaseServiceMock.Setup(x => x.GetUserStatus(username))
                .ReturnsAsync("frozen");

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _authService.GenerateToken(username));
        }

        [Fact]
        public async Task ValidateToken_ValidToken_ReturnsPrincipal()
        {
            // Arrange
            var username = "testUser";
            var token = await _authService.GenerateToken(username);
            Assert.NotNull(token); // Ensure token generation succeeded

            // Act
            var principal = _authService.ValidateToken(token);

            // Assert
            Assert.NotNull(principal);
            var identity = principal.Identity as ClaimsIdentity;
            Assert.NotNull(identity);
            var nameClaim = identity.FindFirst(ClaimTypes.Name);
            Assert.NotNull(nameClaim);
            Assert.Equal(username, nameClaim.Value);
        }
        
        [Fact]
        public async Task RenewToken_ValidToken_ReturnsNewToken()
        {
            // Arrange
            var username = "testUser";
            var originalToken = await _authService.GenerateToken(username);
            Assert.NotNull(originalToken); // Ensure token generation succeeded

            // Act
            System.Threading.Thread.Sleep(1000); // Ensure a different timestamp
            var newToken = await _authService.RenewToken(originalToken);

            // Assert
            Assert.NotNull(newToken);
            Assert.NotEmpty(newToken);
            Assert.NotEqual(originalToken, newToken);
        }
        
        [Fact]
        public void AddAuth_ConfiguresAuthenticationServices()
        {
            // Arrange
            var dbServiceMock = new Mock<IDatabaseService>();
            var authService = new AuthService(dbServiceMock.Object);
            var services = new ServiceCollection();

            // Act
            authService.AddAuth(services);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var authenticationService = serviceProvider.GetService<IAuthenticationService>();
            Assert.NotNull(authenticationService);
        }

        [Fact]
        public void AuthService_MissingEnvironmentVariables_ThrowsException()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
            Environment.SetEnvironmentVariable("JWT_ISSUER", null);
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);

            // Act & Assert
            var dbServiceMock = new Mock<IDatabaseService>();
            Assert.Throws<InvalidOperationException>(() => new AuthService(dbServiceMock.Object));
        }
    }
}
