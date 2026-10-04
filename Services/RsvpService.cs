using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Services;

public class RsvpService : IRsvpService
{
    private readonly ApplicationDbContext _context;

    public RsvpService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasActiveRsvpAsync(string userId, int eventId)
    {
        return _context.Rsvps.AnyAsync(r => r.UserId == userId && r.EventId == eventId && r.Status == RsvpStatus.Registered);
    }

    public async Task<RsvpResult> RegisterAsync(string userId, int eventId)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null || !ev.IsPubliclyVisible)
        {
            return new RsvpResult(false, "This event is not available for registration.");
        }

        if (!ev.RegistrationRequired)
        {
            return new RsvpResult(false, "This event does not require registration.");
        }

        if (ev.HasEnded)
        {
            return new RsvpResult(false, "This event has already taken place.");
        }

        var existing = await _context.Rsvps.FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);
        if (existing is { Status: RsvpStatus.Registered })
        {
            return new RsvpResult(false, "You are already registered for this event.");
        }

        if (ev.MaxAttendees.HasValue)
        {
            var attendeeCount = await _context.Rsvps.CountAsync(r => r.EventId == eventId && r.Status == RsvpStatus.Registered);
            if (attendeeCount >= ev.MaxAttendees.Value)
            {
                return new RsvpResult(false, "This event has reached its maximum number of attendees.");
            }
        }

        if (existing is not null)
        {
            existing.Status = RsvpStatus.Registered;
            existing.RegisteredAt = DateTime.UtcNow;
        }
        else
        {
            _context.Rsvps.Add(new RSVP { UserId = userId, EventId = eventId, Status = RsvpStatus.Registered });
        }

        await _context.SaveChangesAsync();
        return new RsvpResult(true, null);
    }

    public async Task<RsvpResult> CancelAsync(string userId, int eventId)
    {
        var existing = await _context.Rsvps
            .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId && r.Status == RsvpStatus.Registered);

        if (existing is null)
        {
            return new RsvpResult(false, "You are not registered for this event.");
        }

        existing.Status = RsvpStatus.Cancelled;
        await _context.SaveChangesAsync();
        return new RsvpResult(true, null);
    }

    public Task<List<RSVP>> GetForUserAsync(string userId)
    {
        return _context.Rsvps
            .Include(r => r.Event!).ThenInclude(e => e.Category)
            .Include(r => r.Event!).ThenInclude(e => e.City)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();
    }
}
