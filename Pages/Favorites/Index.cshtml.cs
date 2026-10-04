using CityEventsHub.Helpers;
using CityEventsHub.Services;
using CityEventsHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Favorites;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IFavoriteService _favoriteService;

    public IndexModel(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    public List<EventCardViewModel> Favorites { get; set; } = new();

    public async Task OnGetAsync()
    {
        Favorites = await _favoriteService.GetFavoritesForUserAsync(User.GetUserId()!);
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id)
    {
        await _favoriteService.ToggleAsync(User.GetUserId()!, id);
        return RedirectToPage();
    }
}
