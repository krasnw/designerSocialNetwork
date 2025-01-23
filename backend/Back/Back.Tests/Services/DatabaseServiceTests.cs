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
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings")["DefaultConnection"])
                            .Returns(_testConnectionString);

            _databaseService = new DatabaseService(_mockConfiguration.Object);
        }

        [Fact]
        public async Task ExecuteQueryAsync_WithTransaction_UsesProvidedTransaction()
        {
            // This is an integration test and should be run with a real database
            // For unit testing, we'd need to mock NpgsqlConnection and NpgsqlTransaction
            using var connection = _databaseService.GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@param1", "test" }
                };

                const string query = "SELECT @param1";
                
                using var result = await _databaseService.ExecuteQueryAsync(query, parameters, connection, transaction);
                
                Assert.NotNull(result);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_WithTransaction_UsesProvidedTransaction()
        {
            using var connection = _databaseService.GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@param1", "test" }
                };

                const string query = "SELECT @param1";
                
                var result = await _databaseService.ExecuteNonQueryAsync(query, parameters, connection, transaction);
                
                Assert.IsType<int>(result);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
