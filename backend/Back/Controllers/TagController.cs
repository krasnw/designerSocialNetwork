using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public IActionResult GetAllTags()
    {
        var tags = _tagService.GetAllTags();
        return tags != null ? Ok(tags) : NotFound();
    }

    [HttpGet("type/{type}")]
    public IActionResult GetTagsByType(string type)
    {
        var tags = _tagService.GetAllTags(type);
        return tags != null ? Ok(tags) : NotFound();
    }

    [HttpGet("user/{username}")]
    public IActionResult GetUserTags(string username)
    {
        var tags = _tagService.GetAllUserTags(username);
        return tags != null ? Ok(tags) : NotFound();
    }
}
