using System.Data.Common;
using Back.Models;
using Back.Services;
using Back.Services.Interfaces;
using Moq;
using Npgsql;
using Xunit;

namespace Back.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IDatabaseService> _mockDbService;
        private readonly Mock<IImageService> _mockImageService;  // Add this
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockDbService = new Mock<IDatabaseService>();
            _mockImageService = new Mock<IImageService>();  // Add this
            _userService = new UserService(_mockDbService.Object, _mockImageService.Object);
        }

       

        [Fact]
        public void Login_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            var username = "testUser";
            var password = "Test123Password";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            SetupMockDbForSuccessfulLogin(username, hashedPassword);

            // Act
            var result = _userService.Login(username, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Login_WithInvalidCredentials_ReturnsFalse()
        {
            // Arrange
            var username = "testUser";
            var password = "WrongPassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");

            SetupMockDbForSuccessfulLogin(username, hashedPassword);

            // Act
            var result = _userService.Login(username, password);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Logout_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var username = "testUser";
            var password = "Test123Password";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            SetupMockDbForSuccessfulLogin(username, hashedPassword);

            // Act
            _userService.Login(username, password); // Ensure user is logged in
            
            var result = _userService.Logout(username);

            // Assert
            Assert.True(result);
        }


        [Fact]
        public void GetProfile_ExistingUser_ReturnsUserProfile()
        {
            // Arrange
            var username = "testUser";
            SetupMockDbForUserProfile(username);

            // Act
            var profile = _userService.GetProfile(username);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(username, profile.Username);
        }

        [Fact]
        public async Task EditProfile_WithValidData_ReturnsTrue()  // Make async
        {
            // Arrange
            var username = "testUser";
            var request = new User.EditRequest
            {
                Email = "newemail@example.com",
                Password = "NewTest123Password",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+48123456789",
                Description = "New description",
                ProfileImage = CreateMockFormFile(new byte[] { 0x00 }, "test.png", "image/png"),  // Use mock IFormFile
                AccessFee = 100
            };

            SetupMockDbForSuccessfulEdit();
            SetupMockImageServiceForSuccessfulUpload();  // Add this setup

            // Act
            var result = await _userService.EditProfile(username, request);  // Add await

            // Assert
            Assert.True(result);
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

        private void SetupMockImageServiceForSuccessfulUpload()
        {
            _mockImageService.Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<string>()
            )).ReturnsAsync("test.png");
        }

        private void SetupMockDbForSuccessfulSignUp()
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.Setup(r => r.Read()).Returns(false);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }

        private void SetupMockDbForSuccessfulLogin(string username, string hashedPassword)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.HasRows).Returns(true);
            mockReader.Setup(r => r.GetString(0)).Returns(hashedPassword);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }

        private void SetupMockDbForUserProfile(string username)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.HasRows).Returns(true);

            // Setup for all the columns in UserProfile
            mockReader.Setup(r => r.GetOrdinal("username")).Returns(0);
            mockReader.Setup(r => r.GetString(0)).Returns(username);
            mockReader.Setup(r => r.GetOrdinal("first_name")).Returns(1);
            mockReader.Setup(r => r.GetString(1)).Returns("John");
            mockReader.Setup(r => r.GetOrdinal("last_name")).Returns(2);
            mockReader.Setup(r => r.GetString(2)).Returns("Doe");
            mockReader.Setup(r => r.GetOrdinal("profile_description")).Returns(3);
            mockReader.Setup(r => r.GetString(3)).Returns("Test description");
            mockReader.Setup(r => r.GetOrdinal("profile_picture")).Returns(4);
            mockReader.Setup(r => r.GetString(4)).Returns("profile.png");
            mockReader.Setup(r => r.GetOrdinal("amount")).Returns(5);
            mockReader.Setup(r => r.GetInt32(5)).Returns(100);
            mockReader.Setup(r => r.GetOrdinal("total_likes")).Returns(6);
            mockReader.Setup(r => r.GetInt32(6)).Returns(10);
            mockReader.Setup(r => r.GetOrdinal("completed_tasks")).Returns(7);
            mockReader.Setup(r => r.GetInt32(7)).Returns(5);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }

        private void SetupMockDbForSuccessfulEdit()
        {
            _mockDbService.Setup(db => db.ExecuteNonQuery(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()
            )).Verifiable();
        }
    }
}
