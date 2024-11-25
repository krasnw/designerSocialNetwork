namespace Back.Models;

public class ImageContainer(int id, string mainImage, string[] images)
{
    public int Id { get; set; } = id; // To be used as a reference
    string MainImage { get; set; } = mainImage; // Image that will be displayed in the feed
    string[] Images { get; set; } = images; // Includes the main image
}