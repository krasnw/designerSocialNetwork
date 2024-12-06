using Back.Models;
using Back.Models.PostDto;

namespace Back.Services.Interfaces;

public interface IPostService
{
    Post GetPost(int id);
    List<Post>? GetAllUserPosts(int userId);
    List<Post>? GetAllUserPosts(string username);
    List<Post> GetNewestPosts(int pageNumber = 1, int pageSize = 10, string? tags = null, string? accessType = null);
    bool DeletePost(int id, string username);
}
