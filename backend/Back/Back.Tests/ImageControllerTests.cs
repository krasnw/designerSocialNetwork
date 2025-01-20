using Back.Controllers;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Back.Tests.Controllers
{
    public class ImageControllerTests
    {
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private readonly ImageController _controller;

        public ImageControllerTests()
        {
            _imageServiceMock = new Mock<IImageService>();
            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _controller = new ImageController(_imageServiceMock.Object, _webHostEnvironmentMock.Object);
        }

        private void SetupAuthenticatedUser(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task UploadImage_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            var testUsername = "testUser";
            var testFileName = "test.jpg";
            var formFile = new Mock<IFormFile>();
            
            SetupAuthenticatedUser(testUsername);
            
            _imageServiceMock.Setup(x => x.UploadImageAsync(formFile.Object, testUsername))
                .ReturnsAsync(testFileName);

            // Act
            var result = await _controller.UploadImage(formFile.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImageController.ImageUploadResponse>(okResult.Value);
            Assert.Equal(testFileName, response.Filename);
        }

        [Fact]
        public async Task UploadImage_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var formFile = new Mock<IFormFile>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.UploadImage(formFile.Object);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
        

        [Fact]
        public void GetImage_NonExistingImage_ReturnsNotFound()
        {
            // Arrange
            var testFileName = "nonexistent.jpg";
            _webHostEnvironmentMock.Setup(x => x.WebRootPath)
                .Returns("webroot");

            // Act
            var result = _controller.GetImage(testFileName);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteImage_AuthorizedUser_ReturnsOkResult()
        {
            // Arrange
            var testUsername = "testUser";
            var testFileName = "test.jpg";
            
            SetupAuthenticatedUser(testUsername);
            
            _imageServiceMock.Setup(x => x.DeleteImageAsync(testFileName, testUsername))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteImage(testFileName);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteImage_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var testFileName = "test.jpg";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DeleteImage(testFileName);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetUserImages_AuthorizedUser_ReturnsOkResult()
        {
            // Arrange
            var testUsername = "testUser";
            var expectedImages = new List<string> { "image1.jpg", "image2.jpg" };
            
            SetupAuthenticatedUser(testUsername);
            
            _imageServiceMock.Setup(x => x.GetUserImagesAsync(testUsername))
                .ReturnsAsync(expectedImages);

            // Act
            var result = await _controller.GetUserImages(testUsername);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedImages = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(expectedImages.Count, returnedImages.Count);
        }

        [Fact]
        public async Task GetUserImages_DifferentUser_ReturnsForbid()
        {
            // Arrange
            SetupAuthenticatedUser("user1");

            // Act
            var result = await _controller.GetUserImages("user2");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
