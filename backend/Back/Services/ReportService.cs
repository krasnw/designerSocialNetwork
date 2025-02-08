using Back.Models;
using Back.Services.Interfaces;
using Back.Exceptions;
using Npgsql;
using Dapper;
using Back.Models.UserDto;
using Back.Models.PostDto;

namespace Back.Services
{
    public class ReportService : IReportService
    {
        private readonly string _connectionString;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IConfiguration configuration, ILogger<ReportService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task CreateUserReportAsync(string username, CreateUserReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username cannot be empty", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ArgumentException("Reason cannot be empty", nameof(request));

            using var connection = new NpgsqlConnection(_connectionString);
            try 
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    const string reporterIdSql = "SELECT id FROM api_schema.\"user\" WHERE username = @Username";
                    var reporterId = await connection.QueryFirstOrDefaultAsync<int?>(reporterIdSql, 
                        new { Username = username });
                    
                    if (!reporterId.HasValue)
                        throw new ReportException("Reporter account not found");

                    var reportedId = await connection.QueryFirstOrDefaultAsync<int?>(reporterIdSql, 
                        new { Username = request.Username });
                    
                    if (!reportedId.HasValue)
                        throw new NotFoundReportException($"Reported user '{request.Username}' not found");

                    if (reporterId == reportedId)
                        throw new ReportException("You cannot report yourself");

                    if (!int.TryParse(request.Reason, out int reasonIdValue))
                        throw new ReportException("Invalid reason format - must be a number");

                    const string reasonIdSql = "SELECT id FROM api_schema.reason WHERE id = @ReasonId AND reason_type = 'user'";
                    var reasonId = await connection.QueryFirstOrDefaultAsync<int?>(reasonIdSql, 
                        new { ReasonId = reasonIdValue });
                    
                    if (!reasonId.HasValue)
                        throw new ReportException("Invalid reason specified");

                    const string existingReportSql = @"
                        SELECT COUNT(*) FROM api_schema.user_report 
                        WHERE reporter_id = @ReporterId 
                        AND reported_id = @ReportedId 
                        AND report_date = CURRENT_DATE";
                    
                    var existingReportCount = await connection.ExecuteScalarAsync<int>(existingReportSql, 
                        new { ReporterId = reporterId.Value, ReportedId = reportedId.Value });
                    
                    if (existingReportCount > 0)
                        throw new ReportException("You have already reported this user today");

                    const string sql = @"
                        INSERT INTO api_schema.user_report (reporter_id, reported_id, reason_id, report_date)
                        VALUES (@ReporterId, @ReportedId, @ReasonId, CURRENT_DATE)";

                    await connection.ExecuteAsync(sql, new { 
                        ReporterId = reporterId.Value,
                        ReportedId = reportedId.Value,
                        ReasonId = reasonId.Value
                    }, transaction);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (NotFoundReportException)
            {
                throw;
            }
            catch (PostgresException pgEx)
            {
                _logger.LogError(pgEx, "Database error while creating user report");
                throw new ReportException("Database error occurred", pgEx);
            }
            catch (ReportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating user report");
                throw new ReportException("An unexpected error occurred", ex);
            }
        }

        public async Task CreatePostReportAsync(string username, CreatePostReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.PostId <= 0)
                throw new ArgumentException("Invalid post ID", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ArgumentException("Reason cannot be empty", nameof(request));

            using var connection = new NpgsqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    const string reporterIdSql = "SELECT id FROM api_schema.\"user\" WHERE username = @Username";
                    var reporterId = await connection.QueryFirstOrDefaultAsync<int?>(reporterIdSql, 
                        new { Username = username });
                    
                    if (!reporterId.HasValue)
                        throw new ReportException("Reporter account not found");

                    const string postExistsSql = @"
                        SELECT CASE 
                            WHEN EXISTS (SELECT 1 FROM api_schema.post WHERE id = @PostId) THEN 1
                            ELSE 0
                        END";
                    
                    var postExists = await connection.ExecuteScalarAsync<bool>(postExistsSql, 
                        new { PostId = request.PostId });
                    
                    if (!postExists)
                        throw new ReportException($"Post with ID {request.PostId} not found");

                    if (!int.TryParse(request.Reason, out int reasonIdValue))
                        throw new ReportException("Invalid reason format - must be a number");

                    const string reasonIdSql = "SELECT id FROM api_schema.reason WHERE id = @ReasonId AND reason_type = 'post'";
                    var reasonId = await connection.QueryFirstOrDefaultAsync<int?>(reasonIdSql, 
                        new { ReasonId = reasonIdValue });
                    
                    if (!reasonId.HasValue)
                        throw new ReportException("Invalid reason specified");

                    const string existingReportSql = @"
                        SELECT COUNT(*) FROM api_schema.post_report 
                        WHERE reporter_id = @ReporterId 
                        AND reported_id = @ReportedId 
                        AND report_date = CURRENT_DATE";
                    
                    var existingReportCount = await connection.ExecuteScalarAsync<int>(existingReportSql, 
                        new { ReporterId = reporterId.Value, ReportedId = request.PostId });
                    
                    if (existingReportCount > 0)
                        throw new ReportException("You have already reported this post today");

                    const string sql = @"
                        INSERT INTO api_schema.post_report (reporter_id, reported_id, reason_id, report_date)
                        VALUES (@ReporterId, @ReportedId, @ReasonId, CURRENT_DATE)";

                    await connection.ExecuteAsync(sql, new { 
                        ReporterId = reporterId.Value,
                        ReportedId = request.PostId,
                        ReasonId = reasonId.Value
                    }, transaction);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (PostgresException pgEx)
            {
                _logger.LogError(pgEx, "Database error while creating post report");
                throw new ReportException("Database error occurred", pgEx);
            }
            catch (ReportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating post report");
                throw new ReportException("An unexpected error occurred", ex);
            }
        }

        public async Task<IEnumerable<ReasonResponse>> GetUserReasonsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                const string sql = @"
                    SELECT id as Id, reason_name as ReasonName 
                    FROM api_schema.reason 
                    WHERE reason_type = 'user'
                    ORDER BY id";
                
                return await connection.QueryAsync<ReasonResponse>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user reasons");
                throw new ReportException("Failed to retrieve user reasons", ex);
            }
        }

        public async Task<IEnumerable<ReasonResponse>> GetPostReasonsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                const string sql = @"
                    SELECT id as Id, reason_name as ReasonName 
                    FROM api_schema.reason 
                    WHERE reason_type = 'post'
                    ORDER BY id";
                
                return await connection.QueryAsync<ReasonResponse>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post reasons");
                throw new ReportException("Failed to retrieve post reasons", ex);
            }
        }
    }
}
