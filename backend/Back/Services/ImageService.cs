using Back.Services.Interfaces;

namespace Back.Services;

public class ImageService : IImageService
{
    private readonly string _uploadDirectory;
    private readonly IDatabaseService _databaseService;
    private readonly IUserService _userService;
    private readonly ILogger<ImageService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public ImageService(IDatabaseService databaseService, IUserService userService, IWebHostEnvironment environment, ILogger<ImageService> logger)
    {
        _databaseService = databaseService;
        _userService = userService;
        _logger = logger;
        
        try
        {
            // Fallback to current directory if WebRootPath is null
            var rootPath = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _uploadDirectory = Path.Combine(rootPath, "images");
            _logger.LogInformation("Upload directory set to: {Directory}", _uploadDirectory);
            
            Directory.CreateDirectory(_uploadDirectory); // Creates all directories in path if they don't exist
            _logger.LogInformation("Ensured directory exists: {Directory}", _uploadDirectory);

            // Test write permissions
            var testFile = Path.Combine(_uploadDirectory, "test.txt");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            _logger.LogInformation("Directory write test successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize image service");
            throw;
        }
    }

    private string GenerateSecureImageId()
    {
        var bytes = new byte[12]; // 12 bytes = 16 base64 chars
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
    }

    public async Task<string> UploadImageAsync(IFormFile file, string username)
    {
        _logger.LogInformation("Starting image upload for user: {Username}", username);
        
        if (!IsImageValid(file))
        {
            _logger.LogWarning("Invalid image file attempted to upload by user: {Username}", username);
            throw new ArgumentException("Invalid image file");
        }

        var user = _userService.GetUser(username);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", username);
            throw new ArgumentException("User not found");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var secureId = GenerateSecureImageId();
        var filename = $"{secureId}{extension}";
        var filePath = Path.Combine(_uploadDirectory, filename);

        _logger.LogInformation("Saving file: {Filename} to {FilePath}", filename, filePath);

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var uploadStream = file.OpenReadStream())
            {
                await uploadStream.CopyToAsync(fileStream);
            }

            if (!System.IO.File.Exists(filePath))
            {
                throw new IOException("File was not created successfully");
            }

            _logger.LogInformation("File saved successfully: {Filename}", filename);

            // Save to database
            var query = @"
                INSERT INTO api_schema.image (image_file_path, user_id)
                VALUES (@filepath, (SELECT id FROM api_schema.user WHERE username = @username))";

            var parameters = new Dictionary<string, object>
            {
                { "@filepath", filename },
                { "@username", username }
            };

            _databaseService.ExecuteNonQuery(query, parameters);
            _logger.LogInformation("Database entry created for file: {Filename}", filename);

            return filename;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image: {Filename}", filename);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                _logger.LogInformation("Cleaned up failed upload file: {Filename}", filename);
            }
            throw new Exception($"Failed to save image: {ex.Message}");
        }
    }

    public async Task<byte[]?> GetImageAsync(string filename)
    {
        var filePath = Path.Combine(_uploadDirectory, filename);
        return File.Exists(filePath) ? await File.ReadAllBytesAsync(filePath) : null;
    }

    public async Task<bool> DeleteImageAsync(string filename, string username)
    {
        var query = @"
            DELETE FROM api_schema.image 
            WHERE image_file_path = @filepath 
            AND user_id = (SELECT id FROM api_schema.user WHERE username = @username)
            RETURNING id";

        var parameters = new Dictionary<string, object>
        {
            { "@filepath", filename },
            { "@username", username }
        };

        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out var connection, out var command, parameters);
            if (!reader.HasRows)
            {
                return false;
            }

            var filePath = Path.Combine(_uploadDirectory, filename);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<string>> GetUserImagesAsync(string username)
    {
        var query = @"
            SELECT image_file_path
            FROM api_schema.image
            WHERE user_id = (SELECT id FROM api_schema.user WHERE username = @username)";

        var parameters = new Dictionary<string, object>
        {
            { "@username", username }
        };

        var images = new List<string>();
        using var reader = _databaseService.ExecuteQuery(query, out _, out _, parameters);
        while (reader.Read())
        {
            images.Add(reader.GetString(0));
        }

        return images;
    }

    public bool IsImageValid(IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxFileSizeBytes)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    // Add method to get content type
    public string GetContentType(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
