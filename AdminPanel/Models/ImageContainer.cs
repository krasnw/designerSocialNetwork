namespace AdminPanel.Models;

public class Image
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public int ContainerId { get; set; }
    public int UserId { get; set; }
}

public class ImageContainer
{
    public int Id { get; set; }
    public int AmountOfImages { get; set; }
    public int? MainImageId { get; set; }
    public List<Image> AllImages { get; set; } = new List<Image>();
}
