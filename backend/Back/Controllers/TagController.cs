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
        try
        {
            var formattedType = type.ToLower()
                                  .Replace("_", " ");
            var tags = _tagService.GetAllTags(formattedType);
            return tags.Any() ? Ok(tags) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid tag type: {type}");
        }
    }

    [HttpGet("user/{username}")]
    public IActionResult GetUserTags(string username)
    {
        var tags = _tagService.GetAllUserTags(username);
        return tags != null ? Ok(tags) : NotFound();
    }
}
