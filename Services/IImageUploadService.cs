namespace CityEventsHub.Services;

public record ImageUploadResult(bool Success, string? RelativePath, string? ErrorMessage)
{
    public static ImageUploadResult Ok(string relativePath) => new(true, relativePath, null);
    public static ImageUploadResult Fail(string errorMessage) => new(false, null, errorMessage);
}

/// <summary>
/// Validates and stores uploaded images (event photos, profile pictures) under wwwroot/uploads.
/// </summary>
public interface IImageUploadService
{
    /// <param name="file">The uploaded file.</param>
    /// <param name="subFolder">Sub-folder under wwwroot/uploads to store the file in (e.g. "events", "profiles").</param>
    Task<ImageUploadResult> UploadAsync(IFormFile file, string subFolder);
}
