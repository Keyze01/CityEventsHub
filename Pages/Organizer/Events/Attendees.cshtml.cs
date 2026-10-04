using CityEventsHub.Data;
using CityEventsHub.Helpers;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Organizer.Events;

[Authorize(Roles = "Organizer,Administrator")]
public class AttendeesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AttendeesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Event Event { get; set; } = null!;
    public List<RSVP> Attendees { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.OrganizerId != User.GetUserId() && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        Event = ev;
        Attendees = await _context.Rsvps
            .Include(r => r.User)
            .Where(r => r.EventId == id && r.Status == RsvpStatus.Registered)
            .OrderBy(r => r.RegisteredAt)
            .ToListAsync();

        return Page();
    }
}
