namespace AdminPanel.Models;

public enum ReportType
{
    User = 0,
    Post = 1
}

public enum ReportStatus
{
    pending = 0,
    resolved = 1,
    dismissed = 2
}

public abstract class Report
{
    public int Id { get; set; }
    public ReportType Type { get; set; }
    public string ReportReason { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string? Description { get; set; }
    public int ReporterId { get; set; }
    public string? ReporterUsername { get; set; }
    public ReportStatus Status { get; set; }
}

public class UserReport : Report
{
    public User ReportedUser { get; set; } = null!;
}

public class PostReport : Report
{
    public Post ReportedPost { get; set; } = null!;
    public string PostTitle => ReportedPost?.Title ?? string.Empty;
}
