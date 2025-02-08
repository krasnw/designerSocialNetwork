namespace AdminPanel.Services.Interfaces;
using AdminPanel.Models;

public interface IReportHandlerService
{
    Task<IEnumerable<UserReport>> GetUserReports();
    Task<IEnumerable<PostReport>> GetPostReports();
    Task<bool> DismissUserReport(int reportId);
    Task<bool> DismissPostReport(int reportId);
    Task<bool> DeletePost(int postId);
    Task<bool> FreezeUser(string username);
    Task<bool> UnfreezeUser(string username);
    Task<IEnumerable<UserMiniDTO>> GetFrozenUsers();
    Task<bool> IsFrozen(string username);
}
