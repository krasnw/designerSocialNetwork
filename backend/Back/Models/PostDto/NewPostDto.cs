namespace Back.Models.DTO;

public class NewPostDto
{
    private string author;
    private string title;
    private string content;
    private List<IFormFile> images;
    private IFormFile mainImage;
    private string access;
    private List<string> tags;
}