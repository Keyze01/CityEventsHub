using System.ComponentModel.DataAnnotations;

namespace CityEventsHub.Models;

/// <summary>
/// An event category (Conferences, Concerts, Festivals, etc.).
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Font Awesome icon class, e.g. "fa-solid fa-music".</summary>
    [StringLength(100)]
    public string IconClass { get; set; } = "fa-solid fa-calendar-days";

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
