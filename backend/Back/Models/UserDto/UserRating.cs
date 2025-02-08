namespace Back.Models.UserDto;

public class UserRating
{
    public UserMiniDto User { get; set; }
    public int Likes { get; set; }
    public int Place { get; set; }

    public UserRating()
    {
        User = new UserMiniDto();
    }
}