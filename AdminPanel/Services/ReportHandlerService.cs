namespace AdminPanel.Services;

using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

public class ReportHandlerService : IReportHandlerService
{
    private readonly IDatabaseService _dbService;

    public ReportHandlerService(IDatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<IEnumerable<UserReport>> GetUserReports()
    {
        var query = @"
            SELECT ur.id, r.reason_name as report_reason, 
                   ur.report_date, ur.description,
                   ur.reporter_id, u.username as reporter_username,
                   ur.status,
                   -- Reported user details
                   reported.id as reported_user_id,
                   reported.username as reported_username,
                   reported.email as reported_email,
                   reported.account_status as reported_status,
                   reported.account_level as reported_level,
                   reported.join_date as reported_join_date,
                   reported.profile_picture as reported_profile_image
            FROM api_schema.user_report ur
            LEFT JOIN api_schema.""user"" u ON ur.reporter_id = u.id
            LEFT JOIN api_schema.""user"" reported ON ur.reported_id = reported.id
            LEFT JOIN api_schema.reason r ON ur.reason_id = r.id
            ORDER BY ur.report_date DESC";

        var reports = new List<UserReport>();
        using var reader = await _dbService.ExecuteQueryAsync(query);

        while (await reader.ReadAsync())
        {
            reports.Add(new UserReport
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Type = ReportType.User,
                ReportReason = reader.GetString(reader.GetOrdinal("report_reason")),
                ReportDate = reader.GetDateTime(reader.GetOrdinal("report_date")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("description")),
                ReporterId = reader.GetInt32(reader.GetOrdinal("reporter_id")),
                ReporterUsername = reader.IsDBNull(reader.GetOrdinal("reporter_username")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("reporter_username")),
                Status = Enum.Parse<ReportStatus>(reader.GetString(reader.GetOrdinal("status"))),
                ReportedUser = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("reported_user_id")),
                    Username = reader.GetString(reader.GetOrdinal("reported_username")),
                    Email = reader.GetString(reader.GetOrdinal("reported_email")),
                    AccountStatus = reader.GetString(reader.GetOrdinal("reported_status")),
                    AccountLevel = reader.GetString(reader.GetOrdinal("reported_level")),
                    JoinDate = reader.GetDateTime(reader.GetOrdinal("reported_join_date")),
                    ProfileImage = reader.IsDBNull(reader.GetOrdinal("reported_profile_image"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("reported_profile_image"))
                }
            });
        }

        return reports;
    }

    public async Task<IEnumerable<PostReport>> GetPostReports()
    {
        var query = @"
            SELECT 
                pr.id, 
                r.reason_name as report_reason, 
                pr.report_date, 
                pr.description, 
                pr.reporter_id, 
                u.username as reporter_username,
                pr.status,
                -- Post details
                p.id as post_id,
                p.post_name as post_title,
                p.post_text as content,
                p.post_date as created_at,
                p.likes,
                p.access_level as access,
                -- Author details
                author.id as author_id,
                author.username as author_username,
                author.email as author_email,
                author.first_name as author_first_name,
                author.last_name as author_last_name,
                author.phone_number as author_phone,
                author.account_status as author_status,
                author.account_level as author_level,
                author.join_date as author_join_date,
                author.access_fee as author_fee,
                author.profile_description as author_description,
                author.profile_picture as author_profile_image
            FROM api_schema.post_report pr
            LEFT JOIN api_schema.""user"" u ON pr.reporter_id = u.id
            LEFT JOIN api_schema.post p ON pr.reported_id = p.id
            LEFT JOIN api_schema.""user"" author ON p.user_id = author.id
            LEFT JOIN api_schema.reason r ON pr.reason_id = r.id
            ORDER BY pr.report_date DESC";

        var reports = new List<PostReport>();
        using var reader = await _dbService.ExecuteQueryAsync(query);

        while (await reader.ReadAsync())
        {
            reports.Add(new PostReport
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Type = ReportType.Post,
                ReportReason = reader.GetString(reader.GetOrdinal("report_reason")),
                ReportDate = reader.GetDateTime(reader.GetOrdinal("report_date")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("description")),
                ReporterId = reader.GetInt32(reader.GetOrdinal("reporter_id")),
                ReporterUsername = reader.IsDBNull(reader.GetOrdinal("reporter_username")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("reporter_username")),
                Status = Enum.Parse<ReportStatus>(reader.GetString(reader.GetOrdinal("status"))),
                ReportedPost = new Post(
                    reader.GetInt32(reader.GetOrdinal("post_id")),
                    new User(
                        reader.GetString(reader.GetOrdinal("author_username")),
                        reader.GetString(reader.GetOrdinal("author_email")),
                        reader.IsDBNull(reader.GetOrdinal("author_first_name")) 
                            ? string.Empty 
                            : reader.GetString(reader.GetOrdinal("author_first_name")),
                        reader.IsDBNull(reader.GetOrdinal("author_last_name")) 
                            ? string.Empty 
                            : reader.GetString(reader.GetOrdinal("author_last_name")),
                        reader.IsDBNull(reader.GetOrdinal("author_phone")) 
                            ? string.Empty 
                            : reader.GetString(reader.GetOrdinal("author_phone")),
                        reader.GetDecimal(reader.GetOrdinal("author_fee")),
                        reader.GetString(reader.GetOrdinal("author_status")),
                        reader.GetString(reader.GetOrdinal("author_level")),
                        reader.IsDBNull(reader.GetOrdinal("author_description")) 
                            ? string.Empty 
                            : reader.GetString(reader.GetOrdinal("author_description")),
                        reader.GetDateTime(reader.GetOrdinal("author_join_date")),
                        reader.IsDBNull(reader.GetOrdinal("author_profile_image")) 
                            ? string.Empty 
                            : reader.GetString(reader.GetOrdinal("author_profile_image")))
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("author_id"))
                    },
                    reader.GetString(reader.GetOrdinal("post_title")),
                    reader.IsDBNull(reader.GetOrdinal("content")) 
                        ? string.Empty 
                        : reader.GetString(reader.GetOrdinal("content")),
                    null, // ImageContainer will be loaded separately if needed
                    reader.GetDateTime(reader.GetOrdinal("created_at")),
                    reader.GetInt64(reader.GetOrdinal("likes")),
                    reader.GetString(reader.GetOrdinal("access")),
                    null  // Tags will be loaded separately if needed
                )
            });
        }

        return reports;
    }

    public async Task<bool> UpdateReportStatus(int reportId, string reportType, ReportStatus newStatus)
    {
        var query = @"
            UPDATE api_schema.{report_type}_report 
            SET status = @Status::api_schema.report_status
            WHERE id = @ReportId";

        query = query.Replace("{report_type}", reportType.ToLower());

        var parameters = new Dictionary<string, object>
        {
            { "Status", newStatus.ToString() },
            { "ReportId", reportId }
        };

        await _dbService.ExecuteNonQueryAsync(query, parameters);
        return true;
    }

    public async Task<bool> FreezeUser(int userId)
    {
        var query = @"
            UPDATE api_schema.""user""
            SET account_status = 'frozen'::api_schema.account_status
            WHERE id = @UserId";

        var parameters = new Dictionary<string, object>
        {
            { "UserId", userId }
        };

        await _dbService.ExecuteNonQueryAsync(query, parameters);
        return true;
    }

    public async Task<bool> DeletePost(int postId)
    {
        var query = "DELETE FROM api_schema.post WHERE id = @PostId";

        var parameters = new Dictionary<string, object>
        {
            { "PostId", postId }
        };

        await _dbService.ExecuteNonQueryAsync(query, parameters);
        return true;
    }
}
