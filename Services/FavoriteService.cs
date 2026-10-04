using CityEventsHub.Data;
using CityEventsHub.Models;
using CityEventsHub.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;
    private readonly IEventService _eventService;

    public FavoriteService(ApplicationDbContext context, IEventService eventService)
    {
        _context = context;
        _eventService = eventService;
    }

    public Task<bool> IsFavoritedAsync(string userId, int eventId)
    {
        return _context.Favorites.AnyAsync(f => f.UserId == userId && f.EventId == eventId);
    }

    public async Task<bool> ToggleAsync(string userId, int eventId)
    {
        var existing = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);
        if (existing is not null)
        {
            _context.Favorites.Remove(existing);
            await _context.SaveChangesAsync();
            return false;
        }

        _context.Favorites.Add(new Favorite { UserId = userId, EventId = eventId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<EventCardViewModel>> GetFavoritesForUserAsync(string userId)
    {
        var eventIds = await _context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.EventId)
            .ToListAsync();

        var cards = await _eventService.GetCardsByIdsAsync(eventIds);
        var order = eventIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);
        return cards.OrderBy(c => order[c.Id]).ToList();
    }
}
