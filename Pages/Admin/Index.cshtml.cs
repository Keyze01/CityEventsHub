using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Admin;

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalUsers { get; set; }
    public int TotalOrganizers { get; set; }
    public int PendingEvents { get; set; }
    public int ApprovedEvents { get; set; }
    public int RejectedEvents { get; set; }
    public int TotalCategories { get; set; }
    public int TotalCities { get; set; }
    public List<Event> RecentActivity { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalUsers = await _context.Users.CountAsync();

        var organizerRoleId = await _context.Roles
            .Where(r => r.Name == "Organizer")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();
        TotalOrganizers = organizerRoleId is null
            ? 0
            : await _context.UserRoles.CountAsync(ur => ur.RoleId == organizerRoleId);

        PendingEvents = await _context.Events.CountAsync(e => !e.IsApproved && !e.IsRejected && !e.IsCancelled);
        ApprovedEvents = await _context.Events.CountAsync(e => e.IsApproved);
        RejectedEvents = await _context.Events.CountAsync(e => e.IsRejected);
        TotalCategories = await _context.Categories.CountAsync();
        TotalCities = await _context.Cities.CountAsync();

        RecentActivity = await _context.Events
            .Include(e => e.Organizer)
            .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
            .Take(10)
            .ToListAsync();
    }
}
