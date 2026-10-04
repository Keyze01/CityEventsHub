using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Admin.Cities;

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public NewCityInput Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public List<City> Cities { get; set; } = new();

    public class NewCityInput
    {
        [Required(ErrorMessage = "City name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Region { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }
    }

    public async Task OnGetAsync()
    {
        Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        if (await _context.Cities.AnyAsync(c => c.Name == Input.Name && c.Country == Input.Country))
        {
            ErrorMessage = "That city already exists.";
            return RedirectToPage();
        }

        _context.Cities.Add(new City
        {
            Name = Input.Name,
            Region = Input.Region,
            Country = Input.Country,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude
        });
        await _context.SaveChangesAsync();

        StatusMessage = "City created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city is null)
        {
            return RedirectToPage();
        }

        var inUse = await _context.Events.AnyAsync(e => e.CityId == id);
        if (inUse)
        {
            ErrorMessage = "This city is in use by one or more events and cannot be deleted.";
            return RedirectToPage();
        }

        _context.Cities.Remove(city);
        await _context.SaveChangesAsync();

        StatusMessage = "City deleted.";
        return RedirectToPage();
    }
}
