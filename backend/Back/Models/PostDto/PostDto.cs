using Back.Models;

namespace Back.Models.PostDto;

public class PostDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public UserMiniDto Author { get; set; } = null!;
    public string MainImageFilePath { get; set; }
    public List<string> Images { get; set; }  // Add this line
    public long Likes { get; set; }
    public DateOnly CreatedAt { get; set; }
    public string Access { get; set; }
    public List<string> Tags { get; set; }

    public static PostDto MapToPostDto(Post post) =>
        new()
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Author = new UserMiniDto
            {
                Username = post.Author.Username,
                FirstName = post.Author.FirstName,
                LastName = post.Author.LastName
            },
            MainImageFilePath = post.Images.MainImage.Path,
            Images = post.ImagesNames,  // Add this line
            Likes = post.Likes,
            CreatedAt = post.CreatedAt,
            Access = post.Access,
            Tags = post.Tags.Select(t => t.Name).ToList()
        };

    public static List<PostDto> MapToPostDtoList(List<Post> posts) =>
        posts.Select(MapToPostDto).ToList();
}