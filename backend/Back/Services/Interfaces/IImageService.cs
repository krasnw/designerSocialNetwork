using Back.Models;

namespace Back.Services.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file, string username);
    Task<byte[]?> GetImageAsync(string filename);
    Task<bool> DeleteImageAsync(string filename, string username);
    Task<List<string>> GetUserImagesAsync(string username);
    bool IsImageValid(IFormFile file);
    string GetContentType(string filename);
}