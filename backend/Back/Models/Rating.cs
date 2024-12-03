namespace Back.Models;

public class Rating(int place, Tag tag)
{
    public int Place { get; set; } = place;
    public Tag Tag { get; set; } = tag;
}