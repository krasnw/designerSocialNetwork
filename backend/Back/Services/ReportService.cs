using Back.Models;
using Back.Services.Interfaces;
using Npgsql;
using Dapper;

namespace Back.Services
{
    public class ReportService : IReportService
    {
        private readonly string _connectionString;

        public ReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CreateUserReportAsync(string username, CreateUserReportRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            try 
            {
                // First check if reporter exists
                const string reporterIdSql = "SELECT id FROM api_schema.\"user\" WHERE username = @Username";
                var reporterId = await connection.QueryFirstOrDefaultAsync<int?>(reporterIdSql, new { Username = username });
                if (!reporterId.HasValue)
                {
                    throw new InvalidOperationException("Reporter account not found");
                }

                // Then check if reported user exists
                const string reportedIdSql = "SELECT id FROM api_schema.\"user\" WHERE username = @Username";
                var reportedId = await connection.QueryFirstOrDefaultAsync<int?>(reportedIdSql, new { Username = request.Username });
                if (!reportedId.HasValue)
                {
                    throw new InvalidOperationException($"Reported user '{request.Username}' not found");
                }

                // Check if reason exists
                const string reasonIdSql = "SELECT id FROM api_schema.reason WHERE id = @ReasonId AND reason_type = 'user'";
                var reasonId = await connection.QueryFirstOrDefaultAsync<int?>(reasonIdSql, new { ReasonId = int.Parse(request.Reason) });
                if (!reasonId.HasValue)
                {
                    throw new InvalidOperationException("Invalid reason specified");
                }

                // Check if user is already reported
                const string existingReportSql = @"
                    SELECT COUNT(*) FROM api_schema.user_report 
                    WHERE reporter_id = @ReporterId 
                    AND reported_id = @ReportedId 
                    AND report_date = CURRENT_DATE";
                
                var existingReportCount = await connection.ExecuteScalarAsync<int>(existingReportSql, 
                    new { ReporterId = reporterId.Value, ReportedId = reportedId.Value });
                
                if (existingReportCount > 0)
                {
                    throw new InvalidOperationException("You have already reported this user today");
                }

                const string sql = @"
                    INSERT INTO api_schema.user_report (reporter_id, reported_id, reason_id, description, report_date)
                    VALUES (@ReporterId, @ReportedId, @ReasonId, @Description, CURRENT_DATE)";

                await connection.ExecuteAsync(sql, new { 
                    ReporterId = reporterId.Value,
                    ReportedId = reportedId.Value,
                    ReasonId = reasonId.Value,
                    request.Description
                });
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid reason format");
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw user-friendly exceptions
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create report: {ex.Message}");
            }
        }

        public async Task CreatePostReportAsync(string username, CreatePostReportRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            try
            {
                // Check if reporter exists
                const string reporterIdSql = "SELECT id FROM api_schema.\"user\" WHERE username = @Username";
                var reporterId = await connection.QueryFirstOrDefaultAsync<int?>(reporterIdSql, new { Username = username });
                if (!reporterId.HasValue)
                {
                    throw new InvalidOperationException("Reporter account not found");
                }

                // Check if post exists
                const string postExistsSql = "SELECT COUNT(*) FROM api_schema.post WHERE id = @PostId";
                var postExists = await connection.ExecuteScalarAsync<int>(postExistsSql, new { PostId = request.PostId });
                if (postExists == 0)
                {
                    throw new InvalidOperationException($"Post with ID {request.PostId} not found");
                }

                // Check if reason exists
                const string reasonIdSql = "SELECT id FROM api_schema.reason WHERE id = @ReasonId AND reason_type = 'post'";
                var reasonId = await connection.QueryFirstOrDefaultAsync<int?>(reasonIdSql, new { ReasonId = int.Parse(request.Reason) });
                if (!reasonId.HasValue)
                {
                    throw new InvalidOperationException("Invalid reason specified");
                }

                // Check if post is already reported by this user
                const string existingReportSql = @"
                    SELECT COUNT(*) FROM api_schema.post_report 
                    WHERE reporter_id = @ReporterId 
                    AND reported_id = @ReportedId 
                    AND report_date = CURRENT_DATE";
                
                var existingReportCount = await connection.ExecuteScalarAsync<int>(existingReportSql, 
                    new { ReporterId = reporterId.Value, ReportedId = request.PostId });
                
                if (existingReportCount > 0)
                {
                    throw new InvalidOperationException("You have already reported this post today");
                }

                const string sql = @"
                    INSERT INTO api_schema.post_report (reporter_id, reported_id, reason_id, description, report_date)
                    VALUES (@ReporterId, @ReportedId, @ReasonId, @Description, CURRENT_DATE)";

                await connection.ExecuteAsync(sql, new { 
                    ReporterId = reporterId.Value,
                    ReportedId = request.PostId,
                    ReasonId = reasonId.Value,
                    request.Description
                });
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid reason format");
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw user-friendly exceptions
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create report: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ReasonResponse>> GetUserReasonsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            const string sql = @"
                SELECT id, reason_name 
                FROM api_schema.reason 
                WHERE reason_type = 'user'";
            
            return await connection.QueryAsync<ReasonResponse>(sql);
        }

        public async Task<IEnumerable<ReasonResponse>> GetPostReasonsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            const string sql = @"
                SELECT id, reason_name 
                FROM api_schema.reason 
                WHERE reason_type = 'post'";
            
            return await connection.QueryAsync<ReasonResponse>(sql);
        }

        public async Task<AllReportsResponse> GetAllReportsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            
            const string userReportsQuery = @"
                SELECT 
                    ur.id,
                    reporter.username as reporter_username,
                    reported.username as reported_username,
                    r.reason_name,
                    ur.description,
                    ur.report_date
                FROM api_schema.user_report ur
                JOIN api_schema.""user"" reporter ON ur.reporter_id = reporter.id
                JOIN api_schema.""user"" reported ON ur.reported_id = reported.id
                JOIN api_schema.reason r ON ur.reason_id = r.id";

            const string postReportsQuery = @"
                SELECT 
                    pr.id,
                    u.username as reporter_username,
                    pr.reported_id as post_id,
                    r.reason_name,
                    pr.description,
                    pr.report_date
                FROM api_schema.post_report pr
                JOIN api_schema.""user"" u ON pr.reporter_id = u.id
                JOIN api_schema.reason r ON pr.reason_id = r.id";

            var userReports = await connection.QueryAsync<AllReportsResponse.UserReport>(userReportsQuery);
            var postReports = await connection.QueryAsync<AllReportsResponse.PostReport>(postReportsQuery);

            return new AllReportsResponse
            {
                UserReports = userReports,
                PostReports = postReports
            };
        }
    }
}
