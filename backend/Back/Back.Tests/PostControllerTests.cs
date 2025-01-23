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

        [Fact]
        public void GetPosts_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var posts = new List<Post>
            {
                new Post(1, new User("test", "test@test.com", "password", "Test", "User", "123456789", 0, "active", "user", "", "default.jpg"),
                    "Test Post", "Content", new ImageContainer(1, new Image(1, "test.jpg"), new List<Image>()), DateTime.Now, 0, "public", new List<Tag>(), new List<Rating>())
            };

            _postServiceMock.Setup(x => x.GetNewestPosts(pageNumber, pageSize, null, "public"))
                .Returns(posts);

            // Setup HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.GetPosts(pageNumber, pageSize);

            // Assert
            var okResult = Assert.IsType<JsonResult>(result);
            var returnedPosts = Assert.IsType<List<PostDto>>(okResult.Value);
            Assert.Single(returnedPosts);
        }

        [Fact]
        public void GetPosts_InvalidPageNumber_ReturnsBadRequest()
        {
            // Arrange
            var pageNumber = 0;

            // Act
            var result = _controller.GetPosts(pageNumber);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Page number must be greater than 0.", badRequestResult.Value);
        }

        [Fact]
        public void GetPost_ExistingPublicPost_ReturnsOkResult()
        {
            // Arrange
            var postId = 1L;
            var post = new Post(postId, 
                new User("test", "test@test.com", "password", "Test", "User", "123456789", 0, "active", "user", "", "default.jpg"),
                "Test Post", "Content", 
                new ImageContainer(1, new Image(1, "test.jpg"), new List<Image>()), 
                DateTime.Now, 0, "public", 
                new List<Tag>(), new List<Rating>());

            _postServiceMock.Setup(x => x.GetPost(postId))
                .Returns(post);

            // Setup HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.GetPost(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPost = Assert.IsType<PostDto>(okResult.Value);
            Assert.Equal(postId, returnedPost.Id);
        }

        [Fact]
        public void GetPost_NonExistentPost_ReturnsNotFound()
        {
            // Arrange
            var postId = 999L;
            _postServiceMock.Setup(x => x.GetPost(postId))
                .Returns((Post)null);

            // Act
            var result = _controller.GetPost(postId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Post not found.", notFoundResult.Value);
        }

        [Fact]
        public void GetUserPosts_ExistingUser_ReturnsOkResult()
        {
            // Arrange
            var username = "testUser";
            var posts = new List<Post>
            {
                new Post(1, 
                    new User(username, "test@test.com", "password", "Test", "User", "123456789", 0, "active", "user", "", "default.jpg"),
                    "Test Post", "Content", 
                    new ImageContainer(1, new Image(1, "test.jpg"), new List<Image>()), 
                    DateTime.Now, 0, "public", 
                    new List<Tag>(), new List<Rating>())
            };

            _postServiceMock.Setup(x => x.GetAllUserPosts(username))
                .Returns(posts);

            // Setup HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.GetUserPosts(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPosts = Assert.IsType<List<PostMini>>(okResult.Value);
            Assert.Single(returnedPosts);
        }

        [Fact]
        public void GetUserPosts_NoPosts_ReturnsNotFound()
        {
            // Arrange
            var username = "testUser";
            _postServiceMock.Setup(x => x.GetAllUserPosts(username))
                .Returns(new List<Post>());

            // Setup HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.GetUserPosts(username);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No posts found for this user.", notFoundResult.Value);
        }

        [Fact]
        public void DeletePost_AuthorizedUser_ReturnsOkResult()
        {
            // Arrange
            var postId = 1L;
            var username = "testUser";
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

            _postServiceMock.Setup(x => x.DeletePost(postId, username))
                .Returns(true);

            // Act
            var result = _controller.DeletePost(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Post deleted successfully", okResult.Value);
        }

        [Fact]
        public void GetProtectedPost_ValidHash_ReturnsOkResult()
        {
            // Arrange
            var hash = "validHash";
            var post = new Post(1, 
                new User("test", "test@test.com", "password", "Test", "User", "123456789", 0, "active", "user", "", "default.jpg"),
                "Test Post", "Content", 
                new ImageContainer(1, new Image(1, "test.jpg"), new List<Image>()), 
                DateTime.Now, 0, "protected", 
                new List<Tag>(), new List<Rating>());

            _postServiceMock.Setup(x => x.GetProtectedPost(hash))
                .Returns(post);

            // Act
            var result = _controller.GetProtectedPost(hash);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPost = Assert.IsType<PostDto>(okResult.Value);
            Assert.Equal(1, returnedPost.Id);
        }
    }
}