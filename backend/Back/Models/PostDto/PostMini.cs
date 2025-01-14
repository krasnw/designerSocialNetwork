namespace Back.Models.PostDto;

public class PostMini
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string MainImageFilePath { get; set; } = null!;
    public long Likes { get; set; }
    
    public static PostMini MapToPostMini(Post post) =>
        new()
        {
            Id = post.Id,
            Title = post.Title,
            MainImageFilePath = post.Images.MainImage.Path,
            Likes = post.Likes
        };
}