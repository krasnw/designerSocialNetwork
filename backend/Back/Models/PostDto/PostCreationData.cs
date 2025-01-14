namespace Back.Models.PostDto;

public class PostCreationData
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> ImagePaths { get; set; } = new();
    public string MainImagePath { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string AccessLevel { get; set; } = "public";
}
