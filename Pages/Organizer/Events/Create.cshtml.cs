using CityEventsHub.Data;
using CityEventsHub.Helpers;
using CityEventsHub.Models;
using CityEventsHub.Services;
using CityEventsHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Organizer.Events;

[Authorize(Roles = "Organizer,Administrator")]
public class CreateModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IImageUploadService _imageUploadService;
    private readonly ApplicationDbContext _context;

    public CreateModel(IEventService eventService, IImageUploadService imageUploadService, ApplicationDbContext context)
    {
        _eventService = eventService;
        _imageUploadService = imageUploadService;
        _context = context;
    }

    [BindProperty]
    public EventFormViewModel Input { get; set; } = new();

    public List<Category> Categories { get; set; } = new();
    public List<City> Cities { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        string? imagePath = null;
        if (Input.ImageFile is not null)
        {
            var uploadResult = await _imageUploadService.UploadAsync(Input.ImageFile, "events");
            if (!uploadResult.Success)
            {
                ModelState.AddModelError(nameof(Input.ImageFile), uploadResult.ErrorMessage!);
                await LoadDropdownsAsync();
                return Page();
            }
            imagePath = uploadResult.RelativePath;
        }

        var newEvent = new Event
        {
            Title = Input.Title,
            Description = Input.Description,
            CategoryId = Input.CategoryId,
            Date = Input.Date.Date,
            Time = Input.Time,
            Venue = Input.Venue,
            CityId = Input.CityId,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            OrganizerId = User.GetUserId()!,
            ContactEmail = Input.ContactEmail,
            PhoneNumber = Input.PhoneNumber,
            RegistrationRequired = Input.RegistrationRequired,
            MaxAttendees = Input.MaxAttendees,
            ImagePath = imagePath
        };

        var id = await _eventService.CreateAsync(newEvent);

        TempData["StatusMessage"] = "Your event has been submitted and is awaiting administrator approval.";
        return RedirectToPage("/Organizer/Events/Index");
    }

    private async Task LoadDropdownsAsync()
    {
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
    }
}
