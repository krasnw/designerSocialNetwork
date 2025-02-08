namespace Back.Models.UserDto;

public class UserMiniDto
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfileImage { get; set; } 
    
    public static UserMiniDto MapFromUser(User user)
    {
        return new UserMiniDto
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfileImage = user.ProfileImage
        };
    }
}
