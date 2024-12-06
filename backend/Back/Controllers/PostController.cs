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
    public IActionResult GetPosts(int pageNumber, int pageSize, string? tags = null, string? accessType = null)
    {
        var user = User.Identity?.Name;
        if (accessType?.Equals("private", StringComparison.OrdinalIgnoreCase) == true && string.IsNullOrEmpty(user))
        {
            return Unauthorized("You must be logged in to view private posts.");
        }

        var posts = _postService.GetNewestPosts(pageNumber, pageSize, tags, accessType);

        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',').Select(tag => tag.Trim()).ToList();
            posts = posts.Where(post => post.Tags.Any(tag => tagList.Contains(tag.Name))).ToList();
        }

        if (!string.IsNullOrEmpty(accessType))
        {
            posts = posts.Where(post => post.Access.Equals(accessType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var postDtos = PostDto.MapToPostDtoList(posts);
        return new JsonResult(postDtos);
    }

    [HttpGet("{id}")]
    public IActionResult GetPost(int id)
    {
        var post = _postService.GetPost(id);
        if (post == null) return NotFound();
        var postFormatted = PostDto.MapToPostDto(_postService.GetPost(id));
        return postFormatted != null ? Ok(postFormatted) : NotFound();
    }

    [HttpGet("profile/{username}/mini")]
    public IActionResult GetUserPosts(string username, string? tags = null, string? accessType = null)
    {
        var user = User.Identity?.Name;
        var posts = _postService.GetAllUserPosts(username);

        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',').Select(tag => tag.Trim()).ToList();
            posts = posts.Where(post => post.Tags.Any(tag => tagList.Contains(tag.Name))).ToList();
        }

        if (!string.IsNullOrEmpty(accessType))
        {
            if (accessType.Equals("private", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(user))
            {
                return Unauthorized("You must be logged in to view private posts.");
            }

            posts = posts.Where(post =>
                !post.Access.Equals("protected", StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrEmpty(accessType) ||
                 post.Access.Equals(accessType, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        var minis = posts.Select(PostMini.MapToPostMini).ToList();
        return minis != null ? Ok(minis) : NotFound();
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