namespace Back.Models;

public class UserPost
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public static UserPost MapToUserPostDto (User user)
    {
        return new UserPost
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}