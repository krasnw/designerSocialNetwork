using Back.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Npgsql;
using Xunit;

namespace Back.Tests.Services
{
    public class DatabaseServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly DatabaseService _databaseService;
        private readonly string _testConnectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpass";

        public DatabaseServiceTests()
        {
            // Setup mock configuration
            _mockConfiguration = new Mock<IConfiguration>();
            // _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings:DefaultConnection").Value)
            //                 .Returns(_testConnectionString);
            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings")["DefaultConnection"])
                            .Returns(_testConnectionString);

            _databaseService = new DatabaseService(_mockConfiguration.Object);
        }

        [Fact]
        public void Constructor_WithValidConfiguration_CreatesInstance()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x.GetSection("ConnectionStrings")["DefaultConnection"])
                .Returns(_testConnectionString);

            // Act
            var service = new DatabaseService(mockConfig.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullConnectionString_ThrowsException()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x.GetSection("ConnectionStrings")["DefaultConnection"])
                     .Returns((string)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new DatabaseService(mockConfig.Object));
        }

    }
}
