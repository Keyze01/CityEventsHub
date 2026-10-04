using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Services;

/// <summary>
/// Periodically scans for approved events starting within the next 24 hours and sends a
/// one-time reminder notification to every user with an active RSVP, avoiding duplicates
/// by checking for an existing EventReminder notification for that user/event pair.
/// </summary>
public class EventReminderBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventReminderBackgroundService> _logger;

    public EventReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<EventReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event reminders.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task SendDueRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var reminderWindowEnd = now.AddHours(24);

        var upcomingEvents = await context.Events
            .Where(e => e.IsApproved && !e.IsRejected && !e.IsCancelled)
            .Where(e => e.Date >= now.Date && e.Date <= reminderWindowEnd.Date)
            .ToListAsync(cancellationToken);

        foreach (var ev in upcomingEvents)
        {
            if (ev.StartsAt < now || ev.StartsAt > reminderWindowEnd)
            {
                continue;
            }

            var registeredUserIds = await context.Rsvps
                .Where(r => r.EventId == ev.Id && r.Status == RsvpStatus.Registered)
                .Select(r => r.UserId)
                .ToListAsync(cancellationToken);

            foreach (var userId in registeredUserIds)
            {
                var alreadyNotified = await context.Notifications.AnyAsync(
                    n => n.UserId == userId && n.RelatedEventId == ev.Id && n.Type == NotificationType.EventReminder,
                    cancellationToken);

                if (!alreadyNotified)
                {
                    await notificationService.NotifyAsync(
                        userId,
                        $"Reminder: \"{ev.Title}\" starts on {ev.StartsAt:MMM dd, yyyy 'at' HH:mm}.",
                        NotificationType.EventReminder,
                        ev.Id);
                }
            }
        }
    }
}
