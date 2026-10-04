using CityEventsHub.Data;
using CityEventsHub.Models;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Admin.Events;

public enum EventStatusFilter
{
    All,
    Pending,
    Approved,
    Rejected,
    Cancelled
}

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEventService _eventService;

    public IndexModel(ApplicationDbContext context, IEventService eventService)
    {
        _context = context;
        _eventService = eventService;
    }

    [FromQuery]
    public EventStatusFilter Status { get; set; } = EventStatusFilter.Pending;

    [BindProperty]
    public string? RejectReason { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public List<Event> Events { get; set; } = new();

    public async Task OnGetAsync()
    {
        var query = _context.Events
            .Include(e => e.Category)
            .Include(e => e.City)
            .Include(e => e.Organizer)
            .AsQueryable();

        query = Status switch
        {
            EventStatusFilter.Pending => query.Where(e => !e.IsApproved && !e.IsRejected && !e.IsCancelled),
            EventStatusFilter.Approved => query.Where(e => e.IsApproved && !e.IsCancelled),
            EventStatusFilter.Rejected => query.Where(e => e.IsRejected),
            EventStatusFilter.Cancelled => query.Where(e => e.IsCancelled),
            _ => query
        };

        Events = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var success = await _eventService.ApproveAsync(id);
        StatusMessage = success ? "The event has been approved." : "Unable to approve this event.";
        return RedirectToPage(new { Status });
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var reason = string.IsNullOrWhiteSpace(RejectReason) ? "No reason provided." : RejectReason;
        var success = await _eventService.RejectAsync(id, reason);
        StatusMessage = success ? "The event has been rejected." : "Unable to reject this event.";
        return RedirectToPage(new { Status });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var success = await _eventService.DeleteAsync(id);
        StatusMessage = success ? "The event has been deleted." : "Unable to delete this event.";
        return RedirectToPage(new { Status });
    }
}
