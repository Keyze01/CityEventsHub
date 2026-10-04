using CityEventsHub.Helpers;
using CityEventsHub.Models;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Account;

[Authorize]
public class MyRsvpsModel : PageModel
{
    private readonly IRsvpService _rsvpService;

    public MyRsvpsModel(IRsvpService rsvpService)
    {
        _rsvpService = rsvpService;
    }

    public List<RSVP> Rsvps { get; set; } = new();

    public async Task OnGetAsync()
    {
        Rsvps = await _rsvpService.GetForUserAsync(User.GetUserId()!);
    }

    public async Task<IActionResult> OnPostCancelAsync(int eventId)
    {
        await _rsvpService.CancelAsync(User.GetUserId()!, eventId);
        return RedirectToPage();
    }
}
