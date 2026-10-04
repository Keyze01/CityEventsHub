using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using CityEventsHub.Helpers;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Organizer;

/// <summary>
/// Lets a logged-in registered user become an event organizer by filling out an
/// organizer profile. Promotion to the Organizer role is immediate; administrators
/// can later mark the profile as verified.
/// </summary>
[Authorize]
public class ApplyModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public ApplyModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool AlreadyOrganizer { get; set; }

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

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.IsInRole("Organizer") || User.IsInRole("Administrator"))
        {
            AlreadyOrganizer = true;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.IsInRole("Organizer") || User.IsInRole("Administrator"))
        {
            AlreadyOrganizer = true;
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        _context.OrganizerProfiles.Add(new OrganizerProfile
        {
            UserId = user.Id,
            OrganizationName = Input.OrganizationName,
            Bio = Input.Bio,
            ContactPhone = Input.ContactPhone
        });
        await _context.SaveChangesAsync();

        await _userManager.AddToRoleAsync(user, "Organizer");
        await _signInManager.RefreshSignInAsync(user);

        TempData["StatusMessage"] = "Congratulations! You are now an organizer and can start creating events.";
        return RedirectToPage("/Organizer/Index");
    }
}
