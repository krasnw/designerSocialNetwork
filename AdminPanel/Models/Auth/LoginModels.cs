namespace AdminPanel.Models.Auth;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string? Token { get; set; }
    public string? Message { get; set; }
}
