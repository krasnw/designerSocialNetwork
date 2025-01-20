using Back.Controllers;
using Back.Models;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Back.Tests.Controllers
{
    public class TagControllerTests
    {
        private readonly Mock<ITagService> _tagServiceMock;
        private readonly TagController _controller;

        public TagControllerTests()
        {
            _tagServiceMock = new Mock<ITagService>();
            _controller = new TagController(_tagServiceMock.Object);
        }

        [Fact]
        public void GetAllTags_ExistingTags_ReturnsOkResult()
        {
            // Arrange
            var expectedTags = new List<Tag>
            {
                new Tag(1, "Button", TagType.UI_ELEMENT),
                new Tag(2, "Blue", TagType.COLOR)
            };

            _tagServiceMock.Setup(x => x.GetAllTags())
                .Returns(expectedTags);

            // Act
            var result = _controller.GetAllTags();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTags = Assert.IsType<List<Tag>>(okResult.Value);
            Assert.Equal(2, returnedTags.Count);
        }

        [Fact]
        public void GetAllTags_NoTags_ReturnsNotFound()
        {
            // Arrange
            _tagServiceMock.Setup(x => x.GetAllTags())
                .Returns((List<Tag>)null);

            // Act
            var result = _controller.GetAllTags();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetTagsByType_ValidType_ReturnsOkResult()
        {
            // Arrange
            var tagType = "UI_ELEMENT";
            var expectedTags = new List<Tag>
            {
                new Tag(1, "Button", TagType.UI_ELEMENT),
                new Tag(2, "Card", TagType.UI_ELEMENT)
            };

            _tagServiceMock.Setup(x => x.GetAllTags("ui element"))
                .Returns(expectedTags);

            // Act
            var result = _controller.GetTagsByType(tagType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTags = Assert.IsType<List<Tag>>(okResult.Value);
            Assert.Equal(2, returnedTags.Count);
        }

        [Fact]
        public void GetTagsByType_NoTagsFound_ReturnsNotFound()
        {
            // Arrange
            var tagType = "UI_ELEMENT";
            _tagServiceMock.Setup(x => x.GetAllTags("ui element"))
                .Returns(new List<Tag>());

            // Act
            var result = _controller.GetTagsByType(tagType);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetTagsByType_InvalidType_ReturnsBadRequest()
        {
            // Arrange
            var tagType = "INVALID_TYPE";
            _tagServiceMock.Setup(x => x.GetAllTags("invalid type"))
                .Throws(new Exception("Invalid tag type"));

            // Act
            var result = _controller.GetTagsByType(tagType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Invalid tag type: {tagType}", badRequestResult.Value);
        }

        [Fact]
        public void GetUserTags_ExistingUser_ReturnsOkResult()
        {
            // Arrange
            var username = "testUser";
            var expectedTags = new List<Tag>
            {
                new Tag(1, "Button", TagType.UI_ELEMENT),
                new Tag(2, "Blue", TagType.COLOR)
            };

            _tagServiceMock.Setup(x => x.GetAllUserTags(username))
                .Returns(expectedTags);

            // Act
            var result = _controller.GetUserTags(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTags = Assert.IsType<List<Tag>>(okResult.Value);
            Assert.Equal(2, returnedTags.Count);
        }

        [Fact]
        public void GetUserTags_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var username = "nonExistingUser";
            _tagServiceMock.Setup(x => x.GetAllUserTags(username))
                .Returns((List<Tag>)null);

            // Act
            var result = _controller.GetUserTags(username);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
