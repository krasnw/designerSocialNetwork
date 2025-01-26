using Back.Models;

namespace Back.Services.Interfaces
{
    public interface IReportService
    {
        Task CreateUserReportAsync(string username, CreateUserReportRequest request);
        Task CreatePostReportAsync(string username, CreatePostReportRequest request);
        Task<IEnumerable<ReasonResponse>> GetUserReasonsAsync();
        Task<IEnumerable<ReasonResponse>> GetPostReasonsAsync();
        Task<AllReportsResponse> GetAllReportsAsync();
    }
}
