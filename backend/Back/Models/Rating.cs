namespace Back.Models;

public class Rating(int place, string tag)
{
    public int Place { get; set; } = place;
    public string Tag { get; set; } = tag;
}