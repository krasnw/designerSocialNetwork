using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Back.Services;
using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace Back.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly string originalSecretKey;
        private readonly string originalIssuer;
        private readonly string originalAudience;

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
        }

        public void Dispose()
        {
            // Restore original environment variables
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", originalSecretKey);
            Environment.SetEnvironmentVariable("JWT_ISSUER", originalIssuer);
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", originalAudience);
        }

        [Fact]
        public void GenerateToken_ValidUsername_ReturnsToken()
        {
            // Arrange
            var authService = new AuthService();
            var username = "testUser";

            // Act
            var token = authService.GenerateToken(username);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void ValidateToken_ValidToken_ReturnsPrincipal()
        {
            // Arrange
            var authService = new AuthService();
            var username = "testUser";
            var token = authService.GenerateToken(username);
            Assert.NotNull(token); // Ensure token generation succeeded

            // Act
            var principal = authService.ValidateToken(token);

            // Assert
            Assert.NotNull(principal);
            var identity = principal.Identity as ClaimsIdentity;
            Assert.NotNull(identity);
            var nameClaim = identity.FindFirst(ClaimTypes.Name);
            Assert.NotNull(nameClaim);
            Assert.Equal(username, nameClaim.Value);
        }
        
        [Fact]
        public void RenewToken_ValidToken_ReturnsNewToken()
        {
            // Arrange
            var authService = new AuthService();
            var username = "testUser";
            var originalToken = authService.GenerateToken(username);
            Assert.NotNull(originalToken); // Ensure token generation succeeded

            // Act
            System.Threading.Thread.Sleep(1000); // Ensure a different timestamp
            var newToken = authService.RenewToken(originalToken);

            // Assert
            Assert.NotNull(newToken);
            Assert.NotEmpty(newToken);
            Assert.NotEqual(originalToken, newToken);
        }
        
        [Fact]
        public void AddAuth_ConfiguresAuthenticationServices()
        {
            // Arrange
            var authService = new AuthService();
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
            Assert.Throws<InvalidOperationException>(() => new AuthService());
        }
    }
}
