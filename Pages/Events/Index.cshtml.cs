using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using CityEventsHub.Models;
using CityEventsHub.Services;
using CityEventsHub.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Events;

/// <summary>
/// Public event browse/search page. Accessible to guests (no authentication required).
/// </summary>
public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ApplicationDbContext _context;

    public IndexModel(IEventService eventService, ApplicationDbContext context)
    {
        _eventService = eventService;
        _context = context;
    }

    [FromQuery]
    public string? Keyword { get; set; }

    [FromQuery]
    public int? CategoryId { get; set; }

    [FromQuery]
    public int? CityId { get; set; }

    [FromQuery]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [FromQuery]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [FromQuery]
    public string? OrganizerName { get; set; }

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    public PagedResult<EventCardViewModel> Results { get; set; } = new([], 0, 1, 9);
    public List<Category> Categories { get; set; } = new();
    public List<City> Cities { get; set; } = new();

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();

        var filter = new EventSearchViewModel
        {
            Keyword = Keyword,
            CategoryId = CategoryId,
            CityId = CityId,
            FromDate = FromDate,
            ToDate = ToDate,
            OrganizerName = OrganizerName,
            Page = PageNumber,
            PageSize = 9
        };

        Results = await _eventService.SearchAsync(filter);
    }
}
