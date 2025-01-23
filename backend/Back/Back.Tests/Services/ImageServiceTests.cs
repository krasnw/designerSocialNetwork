using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data.Common;
using Npgsql;
using Xunit;

namespace Back.Tests.Services
{
    public class ImageServiceTests
    {
        private readonly Mock<IDatabaseService> _mockDbService;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<ILogger<ImageService>> _mockLogger;
        private readonly string _testDirectory;
        private readonly string _testImageDirectory;
        private readonly ImageService _imageService;

        public ImageServiceTests()
        {
            _mockDbService = new Mock<IDatabaseService>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<ImageService>>();
            
            _testDirectory = Path.Combine(Path.GetTempPath(), "TestImages");
            _testImageDirectory = Path.Combine(_testDirectory, "images");
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDirectory);

            _imageService = new ImageService(
                _mockDbService.Object,
                _mockEnvironment.Object,
                _mockLogger.Object
            );

            // Ensure test directory exists and is empty
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
            Directory.CreateDirectory(_testDirectory);
            Directory.CreateDirectory(_testImageDirectory);
        }

        [Fact]
        public async Task UploadImageAsync_WithValidImage_ReturnsFilename()
        {
            // Arrange
            var username = "testUser";
            var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG file header
            var file = CreateMockFormFile(content, "test.png", "image/png");
            
            SetupMockDbForSuccessfulUpload();
            SetupMockDbForUserExists(username);

            // Act
            var result = await _imageService.UploadImageAsync(file, username);

            // Assert
            Assert.NotNull(result);
            Assert.EndsWith(".png", result);
            Assert.True(File.Exists(Path.Combine(_testImageDirectory, result)));
        }

        [Fact]
        public async Task UploadImageAsync_WithInvalidUser_ThrowsArgumentException()
        {
            // Arrange
            var username = "nonexistentUser";
            var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var file = CreateMockFormFile(content, "test.png", "image/png");
            
            SetupMockDbForUserNotExists();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _imageService.UploadImageAsync(file, username));
        }

        [Theory]
        [InlineData("test.txt", "text/plain")]
        [InlineData("test.exe", "application/octet-stream")]
        public async Task UploadImageAsync_WithInvalidFileType_ThrowsArgumentException(
            string filename, string contentType)
        {
            // Arrange
            var username = "testUser";
            var content = new byte[] { 0x00 };
            var file = CreateMockFormFile(content, filename, contentType);
            
            SetupMockDbForUserExists(username);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _imageService.UploadImageAsync(file, username));
        }

        [Fact]
        public async Task GetImageAsync_ExistingImage_ReturnsImageBytes()
        {
            // Arrange
            var filename = "test.png";
            var expectedContent = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var filePath = Path.Combine(_testDirectory, filename);
            await File.WriteAllBytesAsync(filePath, expectedContent);

            // Act
            var result = await _imageService.GetImageAsync(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task GetImageAsync_NonexistentImage_ReturnsNull()
        {
            // Arrange
            var filename = "nonexistent.png";

            // Act
            var result = await _imageService.GetImageAsync(filename);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteImageAsync_ExistingImage_ReturnsTrue()
        {
            // Arrange
            var username = "testUser";
            var filename = "test.png";
            var filePath = Path.Combine(_testDirectory, filename);
            await File.WriteAllBytesAsync(filePath, new byte[] { 0x00 });

            SetupMockDbForSuccessfulDelete();

            // Act
            var result = await _imageService.DeleteImageAsync(filePath, username);

            // Assert
            Assert.True(result);
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public async Task GetUserImagesAsync_ReturnsImageList()
        {
            // Arrange
            var username = "testUser";
            var expectedImages = new[] { "image1.png", "image2.png" };
            
            SetupMockDbForUserImages(expectedImages);

            // Act
            var result = await _imageService.GetUserImagesAsync(username);

            // Assert
            Assert.Equal(expectedImages, result);
        }

        [Theory]
        [InlineData("test.jpg", true)]
        [InlineData("test.png", true)]
        [InlineData("test.txt", false)]
        [InlineData("test.exe", false)]
        public void IsImageValid_ChecksFileTypeCorrectly(string filename, bool expected)
        {
            // Arrange
            var content = new byte[] { 0x00 };
            var file = CreateMockFormFile(content, filename, "application/octet-stream");

            // Act
            var result = _imageService.IsImageValid(file);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test.jpg", "image/jpeg")]
        [InlineData("test.png", "image/png")]
        [InlineData("test.gif", "image/gif")]
        [InlineData("test.txt", "application/octet-stream")]
        public void GetContentType_ReturnsCorrectType(string filename, string expected)
        {
            // Act
            var result = _imageService.GetContentType(filename);

            // Assert
            Assert.Equal(expected, result);
        }

        private IFormFile CreateMockFormFile(byte[] content, string filename, string contentType)
        {
            var stream = new MemoryStream(content);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(filename);
            fileMock.Setup(f => f.Length).Returns(content.Length);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            return fileMock.Object;
        }

        private void SetupMockDbForSuccessfulUpload()
        {
            _mockDbService.Setup(db => db.ExecuteNonQuery(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()
            )).Verifiable();
        }

        private void SetupMockDbForSuccessfulDelete()
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.HasRows).Returns(true);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }

        private void SetupMockDbForUserImages(string[] images)
        {
            var mockReader = new Mock<DbDataReader>();
            var sequence = mockReader.SetupSequence(r => r.Read());
            foreach (var _ in images)
                sequence.Returns(true);
            sequence.Returns(false);

            var imageIndex = 0;
            mockReader.Setup(r => r.GetString(0))
                .Returns(() => images[imageIndex++]);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }

        private void SetupMockDbForUserExists(string username)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.GetInt32(0)).Returns(1);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.Is<string>(s => s.Contains("SELECT id FROM api_schema.user")),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.Is<Dictionary<string, object>>(d => d.ContainsKey("@username"))
            )).Returns(mockReader.Object);
        }

        private void SetupMockDbForUserNotExists()
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.Setup(r => r.Read()).Returns(false);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.Is<string>(s => s.Contains("SELECT id FROM api_schema.user")),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }
    }
}
