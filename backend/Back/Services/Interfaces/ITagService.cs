using Back.Models;

namespace Back.Services.Interfaces;

public interface ITagService
{
    List<Tag> GetAllTags();
    List<Tag> GetAllTags(string type);
    List<Tag> GetAllUserTags(string username);
    List<Tag> GetPostTags(int postId);
}
