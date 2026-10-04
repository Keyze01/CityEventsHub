using CityEventsHub.Data;
using CityEventsHub.Helpers;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Account;

/// <summary>
/// The registered user's personal dashboard: profile summary, favorite/RSVP/notification counts.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public ApplicationUser CurrentUser { get; set; } = null!;
    public int FavoriteCount { get; set; }
    public int UpcomingRsvpCount { get; set; }
    public int UnreadNotificationCount { get; set; }
    public int ReviewCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.GetUserId()!;
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        CurrentUser = user;
        FavoriteCount = await _context.Favorites.CountAsync(f => f.UserId == userId);
        UpcomingRsvpCount = await _context.Rsvps
            .CountAsync(r => r.UserId == userId
                && r.Status == RsvpStatus.Registered
                && r.Event!.Date >= DateTime.UtcNow.Date);
        UnreadNotificationCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        ReviewCount = await _context.Reviews.CountAsync(r => r.UserId == userId);

        return Page();
    }
}
