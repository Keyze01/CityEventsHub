using CityEventsHub.Data;
using CityEventsHub.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Organizer;

[Authorize(Roles = "Organizer,Administrator")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int PendingApproval { get; set; }
    public int CancelledEvents { get; set; }
    public int RegisteredAttendees { get; set; }

    public async Task OnGetAsync()
    {
        var organizerId = User.GetUserId()!;
        var today = DateTime.UtcNow.Date;

        var events = _context.Events.Where(e => e.OrganizerId == organizerId);

        TotalEvents = await events.CountAsync();
        UpcomingEvents = await events.CountAsync(e => e.Date >= today && !e.IsCancelled);
        PendingApproval = await events.CountAsync(e => !e.IsApproved && !e.IsRejected && !e.IsCancelled);
        CancelledEvents = await events.CountAsync(e => e.IsCancelled);
        RegisteredAttendees = await _context.Rsvps
            .CountAsync(r => r.Event!.OrganizerId == organizerId && r.Status == Models.RsvpStatus.Registered);
    }
}
