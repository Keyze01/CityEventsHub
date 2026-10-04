namespace CityEventsHub.Helpers;

/// <summary>
/// Finds the photo for a city under wwwroot/images/cities. The file is named after the city
/// in lower case (e.g. "Garowe" -> garowe.webp); cities without a photo return null.
/// </summary>
public static class CityImages
{
    private static readonly string[] Extensions = { ".webp", ".jpg", ".jpeg", ".png" };

    public static string? GetPath(IWebHostEnvironment environment, string cityName)
    {
        var slug = new string(cityName.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());

        foreach (var extension in Extensions)
        {
            if (File.Exists(Path.Combine(environment.WebRootPath, "images", "cities", slug + extension)))
            {
                return $"/images/cities/{slug}{extension}";
            }
        }

        return null;
    }
}
