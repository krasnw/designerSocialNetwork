using Back.Models;

namespace Back.Models.DTO;

public class PostDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public UserPostDto Author { get; set; }
    public string MainImageFilePath { get; set; }
    public long Likes { get; set; }
    public string CreatedAt { get; set; }
    public string Access { get; set; }
    public List<string> Tags { get; set; }

    public static PostDto MapToPostDto(Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Author = UserPostDto.MapToUserPostDto(post.Author),
            MainImageFilePath = post.Images.MainImage.ImageFilePath,
            Likes = post.Likes,
            CreatedAt = post.CreatedAt.ToString("yyyy-MM-dd"),
            Access = post.Access,
            Tags = post.Tags.Select(tag => tag.Name).ToList()
        };
    }
    
    public static List<PostDto> MapToPostDtoList(List<Post> posts)
    {
        return posts.Select(MapToPostDto).ToList();
    }
}