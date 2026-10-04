using CityEventsHub.Data;
using CityEventsHub.Services;
using CityEventsHub.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ApplicationDbContext _context;

    public IndexModel(IEventService eventService, ApplicationDbContext context)
    {
        _eventService = eventService;
        _context = context;
    }

    public List<EventCardViewModel> FeaturedEvents { get; set; } = new();
    public List<EventCardViewModel> UpcomingEvents { get; set; } = new();
    public List<Models.Category> Categories { get; set; } = new();
    public List<Models.City> Cities { get; set; } = new();

    public async Task OnGetAsync()
    {
        FeaturedEvents = await _eventService.GetFeaturedAsync(3);
        UpcomingEvents = await _eventService.GetUpcomingAsync(6);
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
    }
}
