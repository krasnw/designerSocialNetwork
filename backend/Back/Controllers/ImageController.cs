using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")] // Changed from "api/[controller]" to "[controller]"
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImageController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var filename = await _imageService.UploadImageAsync(file, username);
            return Ok(new { filename });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error uploading image" });
        }
    }

    [HttpGet("{filename}")]
    public async Task<IActionResult> GetImage(string filename)
    {
        var imageData = await _imageService.GetImageAsync(filename);
        if (imageData == null)
        {
            return NotFound();
        }

        return File(imageData, _imageService.GetContentType(filename));
    }

    [HttpDelete("{filename}")]
    [Authorize]
    public async Task<IActionResult> DeleteImage(string filename)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var result = await _imageService.DeleteImageAsync(filename, username);
        return result ? Ok() : NotFound();
    }

    [HttpGet("user/{username}")]
    [Authorize] // Add authorization requirement
    public async Task<IActionResult> GetUserImages(string username)
    {
        // Only allow users to view their own images
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUser) || currentUser != username)
        {
            return Forbid();
        }

        var images = await _imageService.GetUserImagesAsync(username);
        return Ok(images);
    }
}
