namespace AdminPanel.Services.Interfaces;
using AdminPanel.Models;

public class ReportData
{
    public int Id { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string ReportReason { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string? Description { get; set; }
    public int ReportedItemId { get; set; }
    public int ReporterId { get; set; }
    public string? ReporterUsername { get; set; }
    public string? ReportedUsername { get; set; } // For user reports
    public string? PostTitle { get; set; } // For post reports
}

public interface IReportHandlerService
{
    Task<IEnumerable<UserReport>> GetUserReports();
    Task<IEnumerable<PostReport>> GetPostReports();
    Task<bool> UpdateReportStatus(int reportId, string reportType, ReportStatus newStatus);
    Task<bool> FreezeUser(int userId);
    Task<bool> DeletePost(int postId);
}
