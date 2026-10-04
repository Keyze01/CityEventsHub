using CityEventsHub.Helpers;
using CityEventsHub.Models;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Organizer.Events;

[Authorize(Roles = "Organizer,Administrator")]
public class IndexModel : PageModel
{
    private readonly IEventService _eventService;

    public IndexModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public List<Event> MyEvents { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        MyEvents = await _eventService.GetByOrganizerAsync(User.GetUserId()!);
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var isAdmin = User.IsInRole("Administrator");
        var success = await _eventService.CancelAsync(id, User.GetUserId()!, isAdmin);
        StatusMessage = success ? "The event has been cancelled." : "Unable to cancel this event.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var existing = await _eventService.GetEntityByIdAsync(id);
        if (existing is null || (existing.OrganizerId != User.GetUserId() && !User.IsInRole("Administrator")))
        {
            StatusMessage = "Unable to delete this event.";
            return RedirectToPage();
        }

        await _eventService.DeleteAsync(id);
        StatusMessage = "The event has been deleted.";
        return RedirectToPage();
    }
}
