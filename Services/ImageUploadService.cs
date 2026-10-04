namespace CityEventsHub.Services;

public class ImageUploadService : IImageUploadService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _environment;

    public ImageUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<ImageUploadResult> UploadAsync(IFormFile file, string subFolder)
    {
        if (file.Length == 0)
        {
            return ImageUploadResult.Fail("The selected file is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return ImageUploadResult.Fail("The image must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return ImageUploadResult.Fail("Only JPG, PNG, WEBP, and GIF images are allowed.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return ImageUploadResult.Fail("The uploaded file does not appear to be a valid image.");
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/{subFolder}/{fileName}";
        return ImageUploadResult.Ok(relativePath);
    }
}
