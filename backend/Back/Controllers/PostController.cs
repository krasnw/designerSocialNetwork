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

    public PostController(IPostService postService)
    {
        _postService = postService;
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

    [HttpGet("{id}")]
    public IActionResult GetPost(int id)
    {
        var post = _postService.GetPost(id);
        if (post == null) return NotFound("Post not found.");

        var user = User.Identity?.Name;
        
        // Check access for private posts
        if (post.Access.Equals("private", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(user))
            {
                return Unauthorized("You must be logged in to view private posts.");
            }
            
            // Check if user has access to this private post
            if (!_postService.HasUserAccessToPost(user, post.Id))
            {
                return Unauthorized("You don't have access to this private post.");
            }
        }

        var postFormatted = PostDto.MapToPostDto(post);
        return Ok(postFormatted);
    }

    [HttpGet("profile/{username}/mini")]
    public IActionResult GetUserPosts(string username, string? tags = null, string? accessType = null)
    {
        var currentUser = User.Identity?.Name;
        var posts = _postService.GetAllUserPosts(username);
        
        if (posts == null || !posts.Any())
        {
            return NotFound("No posts found for this user.");
        }

        // Filter posts based on access rights
        if (string.IsNullOrEmpty(currentUser))
        {
            // Not logged in - show only public posts
            posts = posts.Where(post => 
                post.Access.Equals("public", StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        else
        {
            // Logged in - show public posts and private posts if access was bought
            posts = posts.Where(post => 
                post.Access.Equals("public", StringComparison.OrdinalIgnoreCase) ||
                (post.Access.Equals("private", StringComparison.OrdinalIgnoreCase) && 
                 _postService.HasUserAccessToPost(currentUser, post.Id))
            ).ToList();
        }

        // Apply additional access type filter if specified
        if (!string.IsNullOrEmpty(accessType))
        {
            posts = posts.Where(post => 
                post.Access.Equals(accessType, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Apply tag filtering if specified
        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',').Select(tag => tag.Trim()).ToList();
            posts = posts.Where(post => 
                post.Tags.Any(tag => tagList.Contains(tag.Name))
            ).ToList();
        }

        if (!posts.Any())
        {
            return NotFound("No posts found matching the specified criteria.");
        }

        var minis = posts.Select(PostMini.MapToPostMini).ToList();
        return Ok(minis);
    }

    //delete post
    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult DeletePost(int id)
    {
        var uniqueName = User.Identity?.Name;
        if (string.IsNullOrEmpty(uniqueName)) return Unauthorized("Blame the token, relog please");

        var isDeleted = _postService.DeletePost(id, uniqueName);
        return isDeleted ? Ok("Post deleted successfully") : BadRequest("Post not found or you are not the author");
    }
}