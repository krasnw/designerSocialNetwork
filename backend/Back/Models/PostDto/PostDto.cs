﻿using Back.Models;

namespace Back.Models.PostDto;

public class PostDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
    public UserMiniDto Author { get; set; } = null!;
    public List<string> Images { get; set; }
    public long Likes { get; set; }
    public DateOnly CreatedAt { get; set; }
    public string Access { get; set; }
    public List<string>? Tags { get; set; }
    public string? ProtectedAccessLink { get; set; }

    public static PostDto MapToPostDto(Post post, string? protectedAccessLink = null) =>
        new()
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
            ProtectedAccessLink = protectedAccessLink
        };

    public static List<PostDto> MapToPostDtoList(List<Post> posts) =>
        posts.Select(p => MapToPostDto(p)).ToList();
}