using Back.Controllers;
using Back.Models.DTO;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Security.Claims;
using Back.Models;
using Back.Models.PostDto;

namespace Back.Tests.Controllers
{
    public class PostControllerTests
    {
        private readonly Mock<IPostService> _postServiceMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly PostController _controller;

        public PostControllerTests()
        {
            _postServiceMock = new Mock<IPostService>();
            _imageServiceMock = new Mock<IImageService>();
            _controller = new PostController(_postServiceMock.Object, _imageServiceMock.Object);
        }

        [Fact]
        public void DeletePost_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var postId = 1;

            // Setup empty identity (unauthorized user)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.DeletePost(postId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Blame the token, relog please", unauthorizedResult.Value);
        }
    }
}