using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Back.Models.PostDto;

public class CreatePostRequest
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    [Required(ErrorMessage = "At least one image is required")]
    public List<IFormFile> Images { get; set; } = new();
    public int MainImageIndex { get; set; }
    public string? AccessLevel { get; set; }
    public List<string>? Tags { get; set; }
}