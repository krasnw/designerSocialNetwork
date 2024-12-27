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
            Console.WriteLine($"Original type: {type}"); // Debug log
            
            // Convert type to match database enum format (e.g., UI_ELEMENT -> ui element)
            var formattedType = type.ToLower()
                                  .Replace("_", " ");
            
            Console.WriteLine($"Formatted type: {formattedType}"); // Debug log
            
            var tags = _tagService.GetAllTags(formattedType);
            
            Console.WriteLine($"Found tags: {tags.Count}"); // Debug log
            return tags.Any() ? Ok(tags) : NotFound();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}"); // Debug log
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
