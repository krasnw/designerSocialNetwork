using Back.Models;
using Back.Services.Interfaces;
using Npgsql;
using Dapper;

namespace Back.Services;

public class RatingService : IRatingService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public RatingService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<bool> Calculate()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // First, calculate total likes for each user
            var sql = @"
                WITH user_likes AS (
                    SELECT p.user_id, COALESCE(SUM(p.likes), 0) as total_likes
                    FROM api_schema.post p
                    GROUP BY p.user_id
                )
                MERGE INTO api_schema.user_rating ur
                USING user_likes ul ON ur.user_id = ul.user_id
                WHEN MATCHED THEN
                    UPDATE SET total_likes = ul.total_likes, last_updated = CURRENT_TIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (user_id, total_likes, last_updated)
                    VALUES (ul.user_id, ul.total_likes, CURRENT_TIMESTAMP)";

            await connection.ExecuteAsync(sql);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<UserRating>> GetRatings(int limit = 10, int offset = 0)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            WITH RankedUsers AS (
                SELECT 
                    u.username,
                    u.first_name,
                    u.last_name,
                    u.profile_picture,
                    ur.total_likes,
                    ROW_NUMBER() OVER (ORDER BY ur.total_likes DESC) as place
                FROM api_schema.user_rating ur
                JOIN api_schema.""user"" u ON u.id = ur.user_id
            )
            SELECT 
                username as ""Username"",
                first_name as ""FirstName"",
                last_name as ""LastName"",
                profile_picture as ""ProfileImage"",
                total_likes as ""Likes"",
                place as ""Place""
            FROM RankedUsers
            ORDER BY place
            LIMIT @Limit OFFSET @Offset";

        var result = await connection.QueryAsync<UserRatingTemp>(sql, new { Limit = limit, Offset = offset });
        return result.Select(r => new UserRating
        {
            User = new UserDetails
            {
                Username = r.Username,
                FirstName = r.FirstName,
                LastName = r.LastName,
                ProfileImage = r.ProfileImage
            },
            Likes = r.Likes,
            Place = r.Place
        }).ToList();
    }

    public async Task<int> GetRatingPosition(string username)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            WITH RankedUsers AS (
                SELECT 
                    u.username,
                    ur.total_likes,
                    ROW_NUMBER() OVER (ORDER BY ur.total_likes DESC) as position
                FROM api_schema.user_rating ur
                JOIN api_schema.""user"" u ON u.id = ur.user_id
            )
            SELECT position 
            FROM RankedUsers 
            WHERE username = @Username";

        var position = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { Username = username });
        return position ?? 0;
    }

    private class UserRatingTemp
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileImage { get; set; }
        public int Likes { get; set; }
        public int Place { get; set; }
    }
}
