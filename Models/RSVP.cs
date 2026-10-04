namespace CityEventsHub.Models;

public enum RsvpStatus
{
    Registered = 0,
    Cancelled = 1
}

/// <summary>
/// A user's registration to attend an event. Unique per (UserId, EventId).
/// </summary>
public class RSVP
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public RsvpStatus Status { get; set; } = RsvpStatus.Registered;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
