using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IWebHostEnvironment _environment;

    public ImageController(IImageService imageService, IWebHostEnvironment environment)
    {
        _imageService = imageService;
        _environment = environment;
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
            return Ok(new ImageUploadResponse { Filename = filename });
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
    public IActionResult GetImage(string filename)
    {
        var path = Path.Combine(_environment.WebRootPath, "images", filename);
        
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var imageFileStream = System.IO.File.OpenRead(path);
        return File(imageFileStream, "image/png"); // Adjust content type as needed
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

    public class ImageUploadResponse
    {
        public string Filename { get; set; }
    }
}
