using System.Runtime.InteropServices.JavaScript;

namespace Back.Models;

public class Post(
    long id,
    User author,
    string title,
    string? content,
    ImageContainer images,
    DateTime createdAt,
    long likes,
    string access,
    List<Tag>? tags
)
{
    public long Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Content { get; set; } = content ?? "";
    public User Author { get; set; } = author;
    public ImageContainer Images { get; set; } = images;
    public long Likes { get; set; } = likes;
    public DateTime CreatedAt { get; init; } = createdAt;
    public string Access { get; set; } = access;
    public List<Tag> Tags { get; set; } = tags ?? new List<Tag>();

    public List<string> ImagesNames => Images?.AllImages?.Select(img => img.Path)?.ToList() ?? new List<string>();

    public override string ToString()
    {
        return $"Post ID: {Id}, User: {Author.Username}, Title: {Title}, Content: {Content}, " +
               $"Likes: {Likes}, Access Level: {Access}, " +
               $"Tags: {string.Join(", ", Tags?.Select(tag => tag.Name) ?? Array.Empty<string>())}, " +
               $"Images: {string.Join(", ", ImagesNames)}";  // Fixed capitalization of String.Join
    }
}