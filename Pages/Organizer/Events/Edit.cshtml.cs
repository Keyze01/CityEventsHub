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
public class EditModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IImageUploadService _imageUploadService;
    private readonly ApplicationDbContext _context;

    public EditModel(IEventService eventService, IImageUploadService imageUploadService, ApplicationDbContext context)
    {
        _eventService = eventService;
        _imageUploadService = imageUploadService;
        _context = context;
    }

    [BindProperty]
    public EventFormViewModel Input { get; set; } = new();

    [BindProperty]
    public int EventId { get; set; }

    public List<Category> Categories { get; set; } = new();
    public List<City> Cities { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var existing = await _eventService.GetEntityByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (existing.OrganizerId != User.GetUserId() && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        EventId = existing.Id;
        Input = new EventFormViewModel
        {
            Title = existing.Title,
            Description = existing.Description,
            CategoryId = existing.CategoryId,
            Date = existing.Date,
            Time = existing.Time,
            Venue = existing.Venue,
            CityId = existing.CityId,
            Latitude = existing.Latitude,
            Longitude = existing.Longitude,
            ContactEmail = existing.ContactEmail,
            PhoneNumber = existing.PhoneNumber,
            RegistrationRequired = existing.RegistrationRequired,
            MaxAttendees = existing.MaxAttendees,
            ExistingImagePath = existing.ImagePath
        };

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await _eventService.GetEntityByIdAsync(EventId);
        if (existing is null)
        {
            return NotFound();
        }

        if (existing.OrganizerId != User.GetUserId() && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            Input.ExistingImagePath = existing.ImagePath;
            await LoadDropdownsAsync();
            return Page();
        }

        string? imagePath = existing.ImagePath;
        if (Input.ImageFile is not null)
        {
            var uploadResult = await _imageUploadService.UploadAsync(Input.ImageFile, "events");
            if (!uploadResult.Success)
            {
                ModelState.AddModelError(nameof(Input.ImageFile), uploadResult.ErrorMessage!);
                Input.ExistingImagePath = existing.ImagePath;
                await LoadDropdownsAsync();
                return Page();
            }
            imagePath = uploadResult.RelativePath;
        }

        var updated = new Event
        {
            Id = EventId,
            Title = Input.Title,
            Description = Input.Description,
            CategoryId = Input.CategoryId,
            Date = Input.Date.Date,
            Time = Input.Time,
            Venue = Input.Venue,
            CityId = Input.CityId,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            ContactEmail = Input.ContactEmail,
            PhoneNumber = Input.PhoneNumber,
            RegistrationRequired = Input.RegistrationRequired,
            MaxAttendees = Input.MaxAttendees,
            ImagePath = imagePath
        };

        await _eventService.UpdateAsync(updated);

        TempData["StatusMessage"] = "The event has been updated.";
        return RedirectToPage("/Organizer/Events/Index");
    }

    private async Task LoadDropdownsAsync()
    {
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
    }
}
