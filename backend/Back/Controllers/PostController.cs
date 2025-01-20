using Back.Models.DTO;
using Back.Models.PostDto;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IImageService _imageService;

    public PostController(IPostService postService, IImageService imageService)
    {
        _postService = postService;
        _imageService = imageService;
    }

    [HttpGet("feed")]
    public IActionResult GetPosts(int pageNumber = 1, int? pageSize = null, string? tags = null, string? accessType = null)
    {
        if (pageNumber < 1) return BadRequest("Page number must be greater than 0.");
        var actualPageSize = pageSize ?? 10;
        if (actualPageSize < 1) return BadRequest("Page size must be greater than 0.");
        
        var user = User.Identity?.Name;

        // Handle private posts access
        if (accessType?.Equals("private", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (string.IsNullOrEmpty(user))
            {
                return Unauthorized("You must be logged in to view private posts.");
            }
            var privatePosts = _postService.GetNewestPosts(pageNumber, actualPageSize, tags, "private");
            if (!privatePosts.Any())
            {
                return NotFound("No private posts found.");
            }
            return new JsonResult(PostDto.MapToPostDtoList(privatePosts));
        }

        // For public/protected posts
        accessType = string.IsNullOrEmpty(accessType) ? "public" : accessType;
        var posts = _postService.GetNewestPosts(pageNumber, actualPageSize, tags, accessType);
        
        if (!posts.Any())
        {
            return NotFound(tags != null 
                ? "No posts found with the specified tags." 
                : "No posts found.");
        }

        return new JsonResult(PostDto.MapToPostDtoList(posts));
    }

    [HttpGet("protected/{hash}")]
    public IActionResult GetProtectedPost(string hash)
    {
        var post = _postService.GetProtectedPost(hash);
        if (post == null) return NotFound("Post not found.");
        
        var postFormatted = PostDto.MapToPostDto(post);
        return Ok(postFormatted);
    }

    [HttpGet("{id}")]
    public IActionResult GetPost(long id)  // Changed from int to long
    {
        var post = _postService.GetPost(id);
        if (post == null) return NotFound("Post not found.");

        var user = User.Identity?.Name;
        
        // Check access for private/protected posts
        if (post.Access.Equals("private", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(user))
            {
                return Unauthorized("You must be logged in to view private posts.");
            }
            
            if (!_postService.HasUserAccessToPost(user, post.Id))
            {
                return Unauthorized("You don't have access to this private post.");
            }
        }
        else if (post.Access.Equals("protected", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound("This post requires a special access link.");
        }

        var postFormatted = PostDto.MapToPostDto(post);
        return Ok(postFormatted);
    }

    [HttpGet("profile/{username}/mini")]
    public IActionResult GetUserPosts(string username, int pageNumber = 1, int pageSize = 10, string? tags = null, string? accessType = null)
    {
        if (pageNumber < 1) return BadRequest("Page number must be greater than 0.");
        if (pageSize < 1) return BadRequest("Page size must be greater than 0.");
        
        var currentUser = User.Identity?.Name;
        var posts = _postService.GetUserPosts(username, currentUser, pageNumber, pageSize, tags, accessType);
        
        if (posts == null || !posts.Any())
        {
            return NotFound("No posts found matching the specified criteria.");
        }

        return Ok(posts);
    }

    //delete post
    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult DeletePost(long id)  // Changed from int to long
    {
        var uniqueName = User.Identity?.Name;
        if (string.IsNullOrEmpty(uniqueName)) return Unauthorized("Blame the token, relog please");

        var isDeleted = _postService.DeletePost(id, uniqueName);
        return isDeleted ? Ok("Post deleted successfully") : BadRequest("Post not found or you are not the author");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostRequest request)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "User authentication required" });

        try
        {
            // Validation checks
            var validationError = ValidateCreatePostRequest(request);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            // Upload all images first
            var uploadedPaths = new List<string>();
            var uploadedImages = new HashSet<string>(); // Track unique image hashes
            try
            {
                foreach (var image in request.Images)
                {
                    if (!_imageService.IsImageValid(image))
                    {
                        return BadRequest(new { 
                            message = $"Invalid image format or size: {image.FileName}",
                            supportedFormats = new[] { "jpg", "jpeg", "png", "gif" },
                            maxSize = "5MB"
                        });
                    }

                    // Generate unique filename based on content
                    var path = await _imageService.UploadImageAsync(image, username);
                    if (!uploadedImages.Add(path))
                    {
                        // Skip duplicate images but keep the path for the post
                        uploadedPaths.Add(path);
                        continue;
                    }
                    uploadedPaths.Add(path);
                }
            }
            catch (Exception ex)
            {
                // Cleanup any uploaded images
                foreach (var path in uploadedPaths)
                {
                    await _imageService.DeleteImageAsync(path, username);
                }
                return StatusCode(500, new { 
                    message = "Failed to upload images",
                    error = ex.Message
                });
            }

            var mainImageIndex = 0;
            if (request.MainImageIndex!=null && request.MainImageIndex >= 0 && request.MainImageIndex < uploadedPaths.Count)
            {
                mainImageIndex = request.MainImageIndex;
            }

            var createRequest = new PostCreationData
            {
                Title = request.Title,
                Content = request.Content,
                ImagePaths = uploadedPaths,
                MainImagePath = uploadedPaths[mainImageIndex],
                Tags = request.Tags ?? new List<string>(),
                AccessLevel = request.AccessLevel
            };

            var post = await _postService.CreatePost(username, createRequest);
            if (post != null)
            {
                string? protectedAccessLink = null;
                if (post.Access.Equals("protected", StringComparison.OrdinalIgnoreCase))
                {
                    var hash = _postService.GenerateProtectedAccessHash(post.Id);
                    protectedAccessLink = $"{Request.Scheme}://{Request.Host}/post/protected/{hash}";
                }
                return Ok(PostDto.MapToPostDto(post, protectedAccessLink));
            }

            // If post creation failed, cleanup uploaded images
            foreach (var path in uploadedPaths)
            {
                await _imageService.DeleteImageAsync(path, username);
            }
            
            return StatusCode(500, new { 
                message = "Failed to create post",
                error = "Database operation failed"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "An unexpected error occurred while creating the post",
                error = ex.Message 
            });
        }
    }

    private string? ValidateCreatePostRequest(CreatePostRequest request)
    {
        if (string.IsNullOrEmpty(request.Title))
            return "Title is required";
            
        if (!request.Images.Any())
            return "At least one image is required";
            
        if (request.Images.Count > 10)
            return "Maximum 10 images allowed";
            
        if (request.MainImageIndex >= request.Images.Count)
            return "Main image index is invalid";
            
        if (!string.IsNullOrEmpty(request.AccessLevel) && 
            request.AccessLevel != "public" && 
            request.AccessLevel != "private" && 
            request.AccessLevel != "protected")
            return "Invalid access level. Must be: public, private, or protected";

        return null;
    }
}