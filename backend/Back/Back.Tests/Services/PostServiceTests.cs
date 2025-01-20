using System.Data.Common;
using Back.Models;
using Back.Models.PostDto;
using Back.Services;
using Back.Services.Interfaces;
using Moq;
using Npgsql;
using Xunit;

namespace Back.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<IDatabaseService> _mockDbService;
        private readonly Mock<ITagService> _mockTagService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            _mockDbService = new Mock<IDatabaseService>();
            _mockTagService = new Mock<ITagService>();
            _mockUserService = new Mock<IUserService>();
            _postService = new PostService(_mockDbService.Object, _mockTagService.Object, _mockUserService.Object);
        }
        

        [Fact]
        public void GetPost_ExistingPost_ReturnsPost()
        {
            // Arrange
            var postId = 1L;
            SetupMockDbForGetPost(postId);

            // Act
            var result = _postService.GetPost(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.Id);
        }
        

        [Fact]
        public void GetNewestPosts_ValidRequest_ReturnsPosts()
        {
            // Arrange
            SetupMockDbForGetNewestPosts();

            // Act
            var result = _postService.GetNewestPosts(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HasUserAccessToPost_WithAccess_ReturnsTrue()
        {
            // Arrange
            var username = "testUser";
            var postId = 1L;
            SetupMockDbForUserAccess(username, postId, true);

            // Act
            var result = _postService.HasUserAccessToPost(username, postId);

            // Assert
            Assert.True(result);
        }

        private void SetupMockDbForSuccessfulPostCreation()
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)  // User query
                .Returns(true)  // Container creation
                .Returns(true)  // Image insertion
                .Returns(true)  // Main image update
                .Returns(true); // Post creation

            mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            mockReader.Setup(r => r.HasRows).Returns(true);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);

            var user = new User("testUser", "test@example.com", "password", "Test", "User", 
                "+1234567890", 0, "active", "user", "Description", "default.png");
            _mockUserService.Setup(s => s.GetUser(It.IsAny<int>())).Returns(user);
        }

        private void SetupMockDbForGetPost(long postId)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.HasRows).Returns(true);

            mockReader.Setup(r => r.GetInt32(0)).Returns((int)postId);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1); // user_id
            mockReader.Setup(r => r.GetString(2)).Returns("Test Post");
            mockReader.Setup(r => r.GetString(3)).Returns("Test Content");
            mockReader.Setup(r => r.GetInt32(4)).Returns(1); // container_id
            mockReader.Setup(r => r.GetDateTime(5)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetInt32(6)).Returns(0); // likes
            mockReader.Setup(r => r.GetString(7)).Returns("public");

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);

            var user = new User("testUser", "test@example.com", "password", "Test", "User", 
                "+1234567890", 0, "active", "user", "Description", "default.png");
            _mockUserService.Setup(s => s.GetUser(It.IsAny<int>())).Returns(user);
        }

        private void SetupMockDbForSuccessfulDelete(long postId, string username)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)  // author ID query
                .Returns(true); // user ID query
            mockReader.Setup(r => r.GetInt32(0)).Returns(1);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }

        private void SetupMockDbForGetNewestPosts()
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.HasRows).Returns(true);

            // Setup basic post data
            mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetString(2)).Returns("Test Post");
            mockReader.Setup(r => r.GetString(3)).Returns("Test Content");
            mockReader.Setup(r => r.GetInt32(4)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(5)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetInt32(6)).Returns(0);
            mockReader.Setup(r => r.GetString(7)).Returns("public");

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);

            var user = new User("testUser", "test@example.com", "password", "Test", "User", 
                "+1234567890", 0, "active", "user", "Description", "default.png");
            _mockUserService.Setup(s => s.GetUser(It.IsAny<int>())).Returns(user);
        }

        private void SetupMockDbForUserAccess(string username, long postId, bool hasAccess)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.GetInt32(0)).Returns(hasAccess ? 1 : 0);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);
        }
    }
}
