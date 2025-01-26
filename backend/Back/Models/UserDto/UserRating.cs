namespace Back.Models;

public class UserRating
{
    public UserDetails User { get; set; }
    public int Likes { get; set; }
    public int Place { get; set; }

    public UserRating()
    {
        User = new UserDetails();
    }
}

public class UserDetails
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfileImage { get; set; }
}