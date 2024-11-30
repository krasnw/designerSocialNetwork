namespace Back.Models.DTO;

public class UserPostDto
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    
    public static UserPostDto MapToUserPostDto (User user)
    {
        return new UserPostDto
        {
            Username = user.Username,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName
        };
    }
}