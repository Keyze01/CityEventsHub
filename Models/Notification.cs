namespace CityEventsHub.Models;

public enum NotificationType
{
    EventApproved,
    EventRejected,
    EventUpdated,
    EventCancelled,
    EventReminder,
    General
}

/// <summary>
/// An in-app notification shown in a user's dashboard (e.g. event approved/rejected/reminder).
/// </summary>
public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.General;

    public bool IsRead { get; set; }

    public int? RelatedEventId { get; set; }
    public Event? RelatedEvent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
