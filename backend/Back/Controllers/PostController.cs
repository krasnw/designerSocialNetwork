using Back.Models.DTO;
using back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private PostService _postService;

    public PostController()
    {
        _postService = new PostService();
    }

    [HttpGet("feed")]
    public IActionResult GetPosts(int pageNumber, int pageSize)
    {
        var posts = PostDto.MapToPostDtoList(_postService.GetNewestPosts(pageNumber, pageSize));
        return new JsonResult(posts);
    }

    [HttpGet("post/{id}")]
    public IActionResult GetPost(int id)
    {
        var post = _postService.GetPost(id);
        return post != null ? Ok(post) : NotFound();
    }
}