namespace Back.Models;

public class UserRating(string username, string rating, Tag tag)
{
    public string Username { get; set; }
    public string Rating { get; set; }
    public Tag Tag { get; set; }
}