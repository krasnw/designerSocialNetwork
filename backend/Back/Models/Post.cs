namespace Back.Models;

public class Post(
    long id,
    string title,
    string content,
    User author,
    ImageContainer images,
    Post.AccessLevel access,
    string[] tags)
{
    // Enum for post access level
    public enum AccessLevel
    {
        Public,
        Protected,
        Private
    }
    
    long Id { get; set; } = id;
    string Title { get; set; } = title;
    string Content { get; set; } = content;
    User Author { get; set; } = author;
    DateTime CreatedAt { get; set; } = DateTime.Now;
    ImageContainer Images { get; set; } = images;
    long Likes { get; set; } = 0;
    AccessLevel Access { get; set; } = access;
    string[] Tags { get; set; } = tags;
    Rating[]? Ratings { get; set; }
}