namespace AdminPanel.Services.Interfaces;

public class UserData
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string AccountLevel { get; set; } = string.Empty;
}

public interface IUserService
{
    Task<UserData?> GetActiveUserByUsername(string username);
}
