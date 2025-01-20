using System.Data.Common;
using Back.Models;
using Back.Services;
using Back.Services.Interfaces;
using Moq;
using Npgsql;
using Xunit;

namespace Back.Tests.Services
{
    public class TagServiceTests
    {
        private readonly Mock<IDatabaseService> _mockDbService;
        private readonly TagService _tagService;

        public TagServiceTests()
        {
            _mockDbService = new Mock<IDatabaseService>();
            _tagService = new TagService(_mockDbService.Object);
        }

        [Fact]
        public void GetAllTags_ReturnsListOfTags()
        {
            // Arrange
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(true)
                .Returns(false);

            mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            mockReader.Setup(r => r.GetString(1)).Returns("Button");
            mockReader.Setup(r => r.GetString(2)).Returns("UI ELEMENT");

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);

            // Act
            var result = _tagService.GetAllTags();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Button", result[0].Name);
            Assert.Equal(TagType.UI_ELEMENT, result[0].TagType);
        }

        [Theory]
        [InlineData("ui element", TagType.UI_ELEMENT)]
        [InlineData("style", TagType.STYLE)]
        [InlineData("color", TagType.COLOR)]
        public void GetAllTags_ByType_ReturnsFilteredTags(string tagType, TagType expectedEnum)
        {
            // Arrange
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            mockReader.Setup(r => r.GetString(1)).Returns("TestTag");
            mockReader.Setup(r => r.GetString(2)).Returns(tagType);

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);

            // Act
            var result = _tagService.GetAllTags(tagType);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedEnum, result[0].TagType);
        }

        [Fact]
        public void GetAllUserTags_ReturnsUserTags()
        {
            // Arrange
            var username = "testUser";
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            mockReader.Setup(r => r.GetString(1)).Returns("UserTag");
            mockReader.Setup(r => r.GetString(2)).Returns("STYLE");

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.Is<Dictionary<string, object>>(d => d["@username"].ToString() == username)
            )).Returns(mockReader.Object);

            // Act
            var result = _tagService.GetAllUserTags(username);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("UserTag", result[0].Name);
            Assert.Equal(TagType.STYLE, result[0].TagType);
        }

        [Fact]
        public void GetPostTags_ReturnsPostTags()
        {
            // Arrange
            var postId = 1;
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            mockReader.Setup(r => r.GetString(1)).Returns("PostTag");
            mockReader.Setup(r => r.GetString(2)).Returns("COLOR");

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.Is<Dictionary<string, object>>(d => (int)d["@post_id"] == postId)
            )).Returns(mockReader.Object);

            // Act
            var result = _tagService.GetPostTags(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("PostTag", result[0].Name);
            Assert.Equal(TagType.COLOR, result[0].TagType);
        }

        [Fact]
        public void GetAllTags_WithInvalidTagType_ReturnsEmptyList()
        {
            // Arrange
            var mockReader = new Mock<DbDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            mockReader.Setup(r => r.GetString(1)).Returns("InvalidTag");
            mockReader.Setup(r => r.GetString(2)).Returns("INVALID_TYPE");

            _mockDbService.Setup(db => db.ExecuteQuery(
                It.IsAny<string>(),
                out It.Ref<NpgsqlConnection>.IsAny,
                out It.Ref<NpgsqlCommand>.IsAny,
                It.IsAny<Dictionary<string, object>>()
            )).Returns(mockReader.Object);

            // Act
            var result = _tagService.GetAllTags();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
