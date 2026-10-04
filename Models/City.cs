using System.ComponentModel.DataAnnotations;

namespace CityEventsHub.Models;

/// <summary>
/// A city that events can take place in, used for the city selector and search filters.
/// </summary>
public class City
{
    public int Id { get; set; }

    [Required(ErrorMessage = "City name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Region { get; set; }

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
