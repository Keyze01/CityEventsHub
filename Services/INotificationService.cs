using CityEventsHub.Models;

namespace CityEventsHub.Services;

/// <summary>
/// Creates and reads in-app notifications shown in a user's dashboard.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(string userId, string message, NotificationType type, int? relatedEventId = null);
    Task<List<Notification>> GetForUserAsync(string userId, int take = 50);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
}
