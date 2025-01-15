using Back.Models;
using Back.Models.PostDto;

namespace Back.Services.Interfaces;

public interface IPostService
{
    Task<Post?> CreatePost(string username, PostCreationData request);
    Post? GetPost(long id);
    List<Post>? GetAllUserPosts(int userId);
    List<Post>? GetAllUserPosts(string username);
    List<Post> GetNewestPosts(int pageNumber = 1, int pageSize = 10, string? tags = null, string? accessType = null);
    bool HasUserAccessToPost(string username, long postId);
    bool DeletePost(long id, string username);
    Post? GetProtectedPost(string hash);
    string GenerateProtectedAccessHash(long postId);
}
