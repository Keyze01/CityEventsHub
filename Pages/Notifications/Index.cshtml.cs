using CityEventsHub.Helpers;
using CityEventsHub.Models;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Notifications;

[Authorize]
public class IndexModel : PageModel
{
    private readonly INotificationService _notificationService;

    public IndexModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public List<Notification> Notifications { get; set; } = new();

    public async Task OnGetAsync()
    {
        Notifications = await _notificationService.GetForUserAsync(User.GetUserId()!);
    }

    public async Task<IActionResult> OnPostMarkReadAsync(int id)
    {
        await _notificationService.MarkAsReadAsync(id, User.GetUserId()!);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        await _notificationService.MarkAllAsReadAsync(User.GetUserId()!);
        return RedirectToPage();
    }
}
