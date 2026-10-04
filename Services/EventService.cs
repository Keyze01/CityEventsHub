using CityEventsHub.Data;
using CityEventsHub.Models;
using CityEventsHub.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public EventService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    private static IQueryable<EventCardViewModel> ProjectToCard(IQueryable<Event> query)
    {
        return query.Select(e => new EventCardViewModel
        {
            Id = e.Id,
            Title = e.Title,
            ImagePath = e.ImagePath,
            CategoryName = e.Category!.Name,
            CityName = e.City!.Name,
            Date = e.Date,
            Time = e.Time,
            Venue = e.Venue,
            OrganizerName = e.Organizer!.FullName,
            RegistrationRequired = e.RegistrationRequired,
            MaxAttendees = e.MaxAttendees,
            AttendeeCount = e.Rsvps.Count(r => r.Status == RsvpStatus.Registered),
            AverageRating = e.Reviews.Any() ? e.Reviews.Average(r => r.Rating) : null,
            ReviewCount = e.Reviews.Count(),
            IsCancelled = e.IsCancelled
        });
    }

    private IQueryable<Event> PublicEvents() =>
        _context.Events.Where(e => e.IsApproved && !e.IsRejected && !e.IsCancelled);

    public async Task<List<EventCardViewModel>> GetFeaturedAsync(int count)
    {
        var now = DateTime.UtcNow;
        var query = PublicEvents().Where(e => e.Date >= now.Date);

        return await ProjectToCard(query)
            .OrderByDescending(c => c.AttendeeCount + c.ReviewCount)
            .ThenBy(c => c.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<EventCardViewModel>> GetUpcomingAsync(int count)
    {
        var now = DateTime.UtcNow;
        var query = PublicEvents().Where(e => e.Date >= now.Date);

        return await ProjectToCard(query)
            .OrderBy(c => c.Date)
            .ThenBy(c => c.Time)
            .Take(count)
            .ToListAsync();
    }

    public async Task<PagedResult<EventCardViewModel>> SearchAsync(EventSearchViewModel filter)
    {
        var query = PublicEvents();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == filter.CategoryId.Value);
        }

        if (filter.CityId.HasValue)
        {
            query = query.Where(e => e.CityId == filter.CityId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(e => e.Date >= filter.FromDate.Value.Date);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(e => e.Date <= filter.ToDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(filter.OrganizerName))
        {
            var organizerName = filter.OrganizerName.Trim();
            query = query.Where(e => e.Organizer!.FullName.Contains(organizerName));
        }

        var totalCount = await query.CountAsync();

        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize <= 0 ? 9 : filter.PageSize;

        var items = await ProjectToCard(query)
            .OrderBy(c => c.Date).ThenBy(c => c.Time)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<EventCardViewModel>(items, totalCount, page, pageSize);
    }

    public Task<Event?> GetEntityByIdAsync(int id)
    {
        return _context.Events
            .Include(e => e.Category)
            .Include(e => e.City)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public Task<EventCardViewModel?> GetCardByIdAsync(int id)
    {
        return ProjectToCard(_context.Events.Where(e => e.Id == id)).FirstOrDefaultAsync();
    }

    public Task<List<EventCardViewModel>> GetCardsByIdsAsync(IEnumerable<int> ids)
    {
        return ProjectToCard(_context.Events.Where(e => ids.Contains(e.Id))).ToListAsync();
    }

    public Task<List<Event>> GetByOrganizerAsync(string organizerId)
    {
        return _context.Events
            .Include(e => e.Category)
            .Include(e => e.City)
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CreateAsync(Event newEvent)
    {
        newEvent.IsApproved = false;
        newEvent.IsRejected = false;
        newEvent.IsCancelled = false;
        newEvent.CreatedAt = DateTime.UtcNow;

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return newEvent.Id;
    }

    public async Task<bool> UpdateAsync(Event updated)
    {
        var existing = await _context.Events.FirstOrDefaultAsync(e => e.Id == updated.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.CategoryId = updated.CategoryId;
        existing.Date = updated.Date;
        existing.Time = updated.Time;
        existing.Venue = updated.Venue;
        existing.CityId = updated.CityId;
        existing.Latitude = updated.Latitude;
        existing.Longitude = updated.Longitude;
        existing.ContactEmail = updated.ContactEmail;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.RegistrationRequired = updated.RegistrationRequired;
        existing.MaxAttendees = updated.MaxAttendees;
        if (!string.IsNullOrEmpty(updated.ImagePath))
        {
            existing.ImagePath = updated.ImagePath;
        }
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await NotifyInterestedUsersAsync(existing, $"The event \"{existing.Title}\" has been updated.", NotificationType.EventUpdated);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            return false;
        }

        _context.Events.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAsync(int id, string requestingUserId, bool isAdmin)
    {
        var existing = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            return false;
        }

        if (!isAdmin && existing.OrganizerId != requestingUserId)
        {
            return false;
        }

        existing.IsCancelled = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await NotifyInterestedUsersAsync(existing, $"The event \"{existing.Title}\" has been cancelled.", NotificationType.EventCancelled);
        return true;
    }

    public async Task<bool> ApproveAsync(int id)
    {
        var existing = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            return false;
        }

        existing.IsApproved = true;
        existing.IsRejected = false;
        existing.RejectionReason = null;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationService.NotifyAsync(
            existing.OrganizerId,
            $"Your event \"{existing.Title}\" has been approved and is now live.",
            NotificationType.EventApproved,
            existing.Id);

        return true;
    }

    public async Task<bool> RejectAsync(int id, string reason)
    {
        var existing = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            return false;
        }

        existing.IsApproved = false;
        existing.IsRejected = true;
        existing.RejectionReason = reason;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationService.NotifyAsync(
            existing.OrganizerId,
            $"Your event \"{existing.Title}\" was rejected. Reason: {reason}",
            NotificationType.EventRejected,
            existing.Id);

        return true;
    }

    public Task<int> GetAttendeeCountAsync(int eventId)
    {
        return _context.Rsvps.CountAsync(r => r.EventId == eventId && r.Status == RsvpStatus.Registered);
    }

    private async Task NotifyInterestedUsersAsync(Event ev, string message, NotificationType type)
    {
        var interestedUserIds = await _context.Rsvps
            .Where(r => r.EventId == ev.Id && r.Status == RsvpStatus.Registered)
            .Select(r => r.UserId)
            .Union(_context.Favorites.Where(f => f.EventId == ev.Id).Select(f => f.UserId))
            .Distinct()
            .ToListAsync();

        foreach (var userId in interestedUserIds)
        {
            await _notificationService.NotifyAsync(userId, message, type, ev.Id);
        }
    }
}
