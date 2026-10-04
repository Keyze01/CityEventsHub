using CityEventsHub.Models;

namespace CityEventsHub.Services;

public record RsvpResult(bool Success, string? ErrorMessage);

public interface IRsvpService
{
    Task<bool> HasActiveRsvpAsync(string userId, int eventId);
    Task<RsvpResult> RegisterAsync(string userId, int eventId);
    Task<RsvpResult> CancelAsync(string userId, int eventId);
    Task<List<RSVP>> GetForUserAsync(string userId);
}
