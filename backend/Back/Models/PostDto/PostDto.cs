using Back.Models;

namespace Back.Models.PostDto;

public class PostDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
    public UserMiniDto Author { get; set; } = null!;
    public List<string> Images { get; set; }
    public long Likes { get; set; }
    public DateTime CreatedAt { get; set; }  // Changed from DateOnly to DateTime
    public string Access { get; set; }
    public List<string>? Tags { get; set; }
    public string? ProtectedAccessLink { get; set; }
    public bool IsLiked { get; set; }

    public static PostDto MapToPostDto(Post post, string? protectedAccessLink = null, bool isLiked = false)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Author = UserMiniDto.MapFromUser(post.Author),
            Images = post.ImagesNames,
            Likes = post.Likes,
            CreatedAt = post.CreatedAt,
            Access = post.Access,
            Tags = post.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
            // Always include protected access link for protected posts
            ProtectedAccessLink = post.Access.Equals("protected", StringComparison.OrdinalIgnoreCase) 
                ? protectedAccessLink 
                : null,
            IsLiked = isLiked
        };
    }

    public static List<PostDto> MapToPostDtoList(List<Post> posts, Dictionary<long, bool> likeStatus)
    {
        return posts.Select(p => MapToPostDto(p, null, likeStatus.GetValueOrDefault(p.Id))).ToList();
    }
}