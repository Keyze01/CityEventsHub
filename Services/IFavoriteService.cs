using CityEventsHub.ViewModels;

namespace CityEventsHub.Services;

public interface IFavoriteService
{
    Task<bool> IsFavoritedAsync(string userId, int eventId);
    Task<bool> ToggleAsync(string userId, int eventId);
    Task<List<EventCardViewModel>> GetFavoritesForUserAsync(string userId);
}
