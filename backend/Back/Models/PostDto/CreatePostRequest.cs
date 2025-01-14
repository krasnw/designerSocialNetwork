namespace Back.Models.PostDto;

public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<IFormFile> Images { get; set; } = new();
    public int MainImageIndex { get; set; } = 0;
    public List<string> Tags { get; set; } = new();
    public string AccessLevel { get; set; } = "public";
}
