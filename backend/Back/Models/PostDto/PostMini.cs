namespace Back.Models.PostDto;

public class PostMini
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string Access { get; set; } = null!;
    public string MainImageFilePath { get; set; } = null!;
    public long Likes { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public bool IsLiked { get; set; }
    
    public static PostMini MapToPostMini(Post post, bool isLiked = false) =>
        new()
        {
            Id = post.Id,
            Title = post.Title,
            Access = post.Access,
            MainImageFilePath = post.Images.MainImage.Path,
            Likes = post.Likes,
            IsLiked = isLiked,
            Tags = post.Tags.Select(t => t.Name).ToList()
        };
}