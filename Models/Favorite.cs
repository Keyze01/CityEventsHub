namespace CityEventsHub.Models;

/// <summary>
/// A bookmarked event for a given user. Unique per (UserId, EventId).
/// </summary>
public class Favorite
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
