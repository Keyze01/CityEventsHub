using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Cities;

/// <summary>Public listing of all cities.</summary>
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<(City City, int EventCount)> Cities { get; set; } = new();

    public async Task OnGetAsync()
    {
        var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        var counts = await _context.Events
            .Where(e => e.IsApproved && !e.IsRejected && !e.IsCancelled)
            .GroupBy(e => e.CityId)
            .Select(g => new { CityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CityId, g => g.Count);

        Cities = cities
            .Select(c => (c, counts.GetValueOrDefault(c.Id, 0)))
            .ToList();
    }
}
