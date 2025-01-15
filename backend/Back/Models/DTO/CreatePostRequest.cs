using System.ComponentModel.DataAnnotations;

public class CreatePostRequest
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; }
    public string? Content { get; set; }
    [Required(ErrorMessage = "At least one image is required")]
    public List<IFormFile> Images { get; set; }
    public int MainImageIndex { get; set; }
    public string? AccessLevel { get; set; }
    public List<string>? Tags { get; set; }
}
