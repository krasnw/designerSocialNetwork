namespace AdminPanel.Services;

using AdminPanel.Services.Interfaces;

public class UserService : IUserService
{
    private readonly IDatabaseService _dbService;

    public UserService(IDatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<UserData?> GetActiveUserByUsername(string username)
    {
        var query = @"
            SELECT id, username, user_password, account_level 
            FROM api_schema.""user"" 
            WHERE username = @Username AND account_status = 'active'";

        var parameters = new Dictionary<string, object>
        {
            { "Username", username }
        };

        using var reader = await _dbService.ExecuteQueryAsync(query, parameters);

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new UserData
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Username = reader.GetString(reader.GetOrdinal("username")),
            Password = reader.GetString(reader.GetOrdinal("user_password")),
            AccountLevel = reader.GetString(reader.GetOrdinal("account_level"))
        };
    }
}
