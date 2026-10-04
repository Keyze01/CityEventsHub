using System.ComponentModel.DataAnnotations;

namespace CityEventsHub.Models;

/// <summary>
/// A 1-5 star rating and optional comment left by a user after an event has ended.
/// Unique per (UserId, EventId).
/// </summary>
public class Review
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
