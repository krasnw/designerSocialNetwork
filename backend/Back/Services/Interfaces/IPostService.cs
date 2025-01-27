using Back.Models;
using Back.Models.PostDto;

namespace Back.Services.Interfaces;

public interface IPostService
{
    Task<Post?> CreatePost(string username, PostCreationData request);
    Post? GetPost(long id);
    List<Post>? GetAllUserPosts(int userId);
    List<Post>? GetAllUserPosts(string username);
    List<Post> GetNewestPosts(int pageNumber = 1, int pageSize = 10, string? tags = null, string? accessType = null, string? currentUser = null);
    bool HasUserAccessToPost(string username, long postId);
    bool DeletePost(long id, string username);
    Post? GetProtectedPost(string hash);
    string GenerateProtectedAccessHash(long postId);
    List<PostMini>? GetUserPosts(string username, string? currentUser, int pageNumber, int pageSize, string? tags = null, string? accessType = null);
    bool LikePost(string username, long postId);
    bool IsPostLikedByUser(string username, long postId);
    List<PostMini>? GetOwnPosts(string username, int pageNumber = 1, int pageSize = 10, string? tags = null, string? accessType = null);
    string? GetProtectedAccessHash(long postId);
}
