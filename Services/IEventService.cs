using CityEventsHub.Models;
using CityEventsHub.ViewModels;

namespace CityEventsHub.Services;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Core event operations: browsing/search for the public, CRUD for organizers,
/// and approval workflow for administrators.
/// </summary>
public interface IEventService
{
    Task<List<EventCardViewModel>> GetFeaturedAsync(int count);
    Task<List<EventCardViewModel>> GetUpcomingAsync(int count);
    Task<PagedResult<EventCardViewModel>> SearchAsync(EventSearchViewModel filter);

    Task<Event?> GetEntityByIdAsync(int id);
    Task<EventCardViewModel?> GetCardByIdAsync(int id);
    Task<List<EventCardViewModel>> GetCardsByIdsAsync(IEnumerable<int> ids);

    Task<List<Event>> GetByOrganizerAsync(string organizerId);

    Task<int> CreateAsync(Event newEvent);
    Task<bool> UpdateAsync(Event updated);
    Task<bool> DeleteAsync(int id);
    Task<bool> CancelAsync(int id, string requestingUserId, bool isAdmin);

    Task<bool> ApproveAsync(int id);
    Task<bool> RejectAsync(int id, string reason);

    Task<int> GetAttendeeCountAsync(int eventId);
}
