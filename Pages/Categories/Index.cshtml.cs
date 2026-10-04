using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Categories;

/// <summary>Public listing of all event categories.</summary>
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<(Category Category, int EventCount)> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        var counts = await _context.Events
            .Where(e => e.IsApproved && !e.IsRejected && !e.IsCancelled)
            .GroupBy(e => e.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CategoryId, g => g.Count);

        Categories = categories
            .Select(c => (c, counts.GetValueOrDefault(c.Id, 0)))
            .ToList();
    }
}
