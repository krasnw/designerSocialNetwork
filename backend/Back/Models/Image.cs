using System.Drawing;

namespace Back.Models;

public class Image
{
    public int Id { get; set; }
    public string Path { get; set; }  // Changed from ImageFilePath to Path
    
    public Image(int id, string imageFilePath)
    {
        Id = id;
        Path = imageFilePath;
    }
}

