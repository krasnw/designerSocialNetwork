namespace Back.Models;

public class ImageContainer(int id, Image mainImage, List<Image> images)
{
    public int Id { get; set; } = id; // To be used as a reference
    public Image MainImage { get; set; } = mainImage; // Image that will be displayed in the feed
    public List<Image> AllImages { get; set; } = images; // Changed from Images to AllImages
}