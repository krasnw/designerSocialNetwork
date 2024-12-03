using System.Drawing;

namespace Back.Models;

public class Image
{
    public int Id { get; set; }
    public string ImageFilePath { get; set; }
    
    public Image(int id, string imageFilePath)
    {
        Id = id;
        ImageFilePath = imageFilePath;
    }
}

