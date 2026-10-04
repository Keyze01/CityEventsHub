using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using CityEventsHub.Helpers;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Organizer;

[Authorize(Roles = "Organizer,Administrator")]
public class ProfileModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ProfileModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Organization name is required.")]
        [StringLength(200)]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Bio { get; set; }

        [Phone]
        [StringLength(30)]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }
    }

    public async Task OnGetAsync()
    {
        var userId = User.GetUserId()!;
        var profile = await _context.OrganizerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is not null)
        {
            Input = new InputModel
            {
                OrganizationName = profile.OrganizationName,
                Bio = profile.Bio,
                ContactPhone = profile.ContactPhone
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.GetUserId()!;
        var profile = await _context.OrganizerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
        {
            profile = new OrganizerProfile { UserId = userId };
            _context.OrganizerProfiles.Add(profile);
        }

        profile.OrganizationName = Input.OrganizationName;
        profile.Bio = Input.Bio;
        profile.ContactPhone = Input.ContactPhone;

        await _context.SaveChangesAsync();

        StatusMessage = "Your organizer profile has been saved.";
        return RedirectToPage();
    }
}
