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
            SELECT ur.id,
                   r.reason_name as report_reason, 
                   ur.report_date,
                   -- Reporter details
                   reporter.username as reporter_username,
                   reporter.first_name as reporter_first_name,
                   reporter.last_name as reporter_last_name,
                   reporter.profile_picture as reporter_profile_image,
                   reporter.profile_description as reporter_description,
                   -- Reported user details
                   reported.username as reported_username,
                   reported.first_name as reported_first_name,
                   reported.last_name as reported_last_name,
                   reported.profile_picture as reported_profile_image,
                   reported.profile_description as reported_description
            FROM api_schema.user_report ur
            JOIN api_schema.""user"" reporter ON ur.reporter_id = reporter.id
            JOIN api_schema.""user"" reported ON ur.reported_id = reported.id
            JOIN api_schema.reason r ON ur.reason_id = r.id
            ORDER BY ur.report_date DESC";

        var reports = new List<UserReport>();
        using var reader = await _dbService.ExecuteQueryAsync(query);

        while (await reader.ReadAsync())
        {
            reports.Add(new UserReport
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                ReportReason = reader.GetString(reader.GetOrdinal("report_reason")),
                ReportDate = reader.GetDateTime(reader.GetOrdinal("report_date")),
                Reporter = new UserMiniDTO(
                    reader.GetString(reader.GetOrdinal("reporter_username")),
                    reader.GetString(reader.GetOrdinal("reporter_first_name")),
                    reader.GetString(reader.GetOrdinal("reporter_last_name")),
                    reader.IsDBNull(reader.GetOrdinal("reporter_profile_image"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("reporter_profile_image")),
                    reader.IsDBNull(reader.GetOrdinal("reporter_description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("reporter_description"))
                ),
                ReportedUser = new UserMiniDTO(
                    reader.GetString(reader.GetOrdinal("reported_username")),
                    reader.GetString(reader.GetOrdinal("reported_first_name")),
                    reader.GetString(reader.GetOrdinal("reported_last_name")),
                    reader.IsDBNull(reader.GetOrdinal("reported_profile_image"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("reported_profile_image")),
                    reader.IsDBNull(reader.GetOrdinal("reported_description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("reported_description"))
                )
            });
        }

        return reports;
    }

    public async Task<IEnumerable<PostReport>> GetPostReports()
    {
        var query = @"
            SELECT pr.id,
                r.reason_name as report_reason,
                pr.report_date,
                -- Reporter details
                reporter.username as reporter_username,
                reporter.first_name as reporter_first_name,
                reporter.last_name as reporter_last_name,
                reporter.profile_picture as reporter_profile_image,
                reporter.profile_description as reporter_description,
                -- Post details
                p.id as post_id,
                p.post_name as post_title,
                p.post_text as content,
                ic.main_image_path,
                -- Post author details
                author.username as author_username,
                author.first_name as author_first_name,
                author.last_name as author_last_name,
                author.profile_picture as author_profile_image,
                author.profile_description as author_description
            FROM api_schema.post_report pr
            JOIN api_schema.""user"" reporter ON pr.reporter_id = reporter.id
            JOIN api_schema.post p ON pr.reported_id = p.id
            JOIN api_schema.""user"" author ON p.user_id = author.id
            LEFT JOIN api_schema.image_container ic ON p.container_id = ic.id
            JOIN api_schema.reason r ON pr.reason_id = r.id
            ORDER BY pr.report_date DESC";

        var reports = new List<PostReport>();
        using var reader = await _dbService.ExecuteQueryAsync(query);

        while (await reader.ReadAsync())
        {
            reports.Add(new PostReport
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                ReportReason = reader.GetString(reader.GetOrdinal("report_reason")),
                ReportDate = reader.GetDateTime(reader.GetOrdinal("report_date")),
                Reporter = new UserMiniDTO(
                    reader.GetString(reader.GetOrdinal("reporter_username")),
                    reader.GetString(reader.GetOrdinal("reporter_first_name")),
                    reader.GetString(reader.GetOrdinal("reporter_last_name")),
                    reader.IsDBNull(reader.GetOrdinal("reporter_profile_image"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("reporter_profile_image")),
                    reader.IsDBNull(reader.GetOrdinal("reporter_description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("reporter_description"))
                ),
                ReportedUser = new UserMiniDTO(
                    reader.GetString(reader.GetOrdinal("author_username")),
                    reader.GetString(reader.GetOrdinal("author_first_name")),
                    reader.GetString(reader.GetOrdinal("author_last_name")),
                    reader.IsDBNull(reader.GetOrdinal("author_profile_image"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("author_profile_image")),
                    reader.IsDBNull(reader.GetOrdinal("author_description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("author_description"))
                ),
                ReportedPost = new PostMiniDTO(
                    reader.GetInt64(reader.GetOrdinal("post_id")),
                    reader.GetString(reader.GetOrdinal("post_title")),
                    reader.IsDBNull(reader.GetOrdinal("content"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("content")),
                    new ImageContainer(
                        reader.IsDBNull(reader.GetOrdinal("main_image_path")) 
                            ? string.Empty 
                            : reader.GetString(reader.GetOrdinal("main_image_path")),
                        new List<string>() // Images list would need another query to populate
                    )
                )
            });
        }

        return reports;
    }

    public async Task<bool> DeletePost(int postId)
    {
        var deleteReportsQuery = "DELETE FROM api_schema.post_report WHERE reported_id = @PostId";
        var deletePostQuery = "DELETE FROM api_schema.post WHERE id = @PostId";
        var parameters = new Dictionary<string, object> { { "PostId", postId } };

        return await _dbService.ExecuteWithConnectionAsync(async connection =>
        {
            using var transaction = connection.BeginTransaction();
            try
            {
                // Delete reports first
                await _dbService.ExecuteNonQueryAsync(deleteReportsQuery, parameters, connection, transaction);

                // Then delete the post
                var result = await _dbService.ExecuteNonQueryAsync(deletePostQuery, parameters, connection, transaction);
                
                await transaction.CommitAsync();
                return result > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<bool> DismissUserReport(int reportId)
    {
        var query = "DELETE FROM api_schema.user_report WHERE id = @ReportId";
        var parameters = new Dictionary<string, object> { { "ReportId", reportId } };
        var rowsAffected = await _dbService.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DismissPostReport(int reportId)
    {
        var query = "DELETE FROM api_schema.post_report WHERE id = @ReportId";
        var parameters = new Dictionary<string, object> { { "ReportId", reportId } };
        var rowsAffected = await _dbService.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> FreezeUser(string username)
    {
        var query = @"
            UPDATE api_schema.""user""
            SET account_status = 'frozen'
            WHERE username = @Username";

        var parameters = new Dictionary<string, object> { { "Username", username } };
        var rowsAffected = await _dbService.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> UnfreezeUser(string username)
    {
        var query = @"
            UPDATE api_schema.""user""
            SET account_status = 'active'
            WHERE username = @Username";

        var parameters = new Dictionary<string, object> { { "Username", username } };
        var rowsAffected = await _dbService.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<UserMiniDTO>> GetFrozenUsers()
    {
        var query = @"
            SELECT 
                username,
                first_name,
                last_name,
                COALESCE(profile_picture, '') as profile_image,
                COALESCE(profile_description, '') as profile_description
            FROM api_schema.""user""
            WHERE account_status = 'frozen'
            ORDER BY username";

        var frozenUsers = new List<UserMiniDTO>();
        using var reader = await _dbService.ExecuteQueryAsync(query);

        while (await reader.ReadAsync())
        {
            frozenUsers.Add(new UserMiniDTO(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4)
            ));
        }

        return frozenUsers;
    }

    public async Task<bool> IsFrozen(string username)
    {
        var query = @"
            SELECT account_status = 'frozen'
            FROM api_schema.""user""
            WHERE username = @Username";

        var parameters = new Dictionary<string, object> { { "Username", username } };
        return await _dbService.ExecuteScalarAsync<bool>(query, parameters);
    }
}
