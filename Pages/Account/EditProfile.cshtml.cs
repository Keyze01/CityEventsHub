using System.ComponentModel.DataAnnotations;
using CityEventsHub.Helpers;
using CityEventsHub.Models;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Account;

[Authorize]
public class EditProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IImageUploadService _imageUploadService;

    public EditProfileModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IImageUploadService imageUploadService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _imageUploadService = imageUploadService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ProfileImagePath { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber
        };
        ProfileImagePath = user.ProfileImagePath;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ProfileImagePath = user.ProfileImagePath;
            return Page();
        }

        user.FullName = Input.FullName;
        user.PhoneNumber = Input.PhoneNumber;

        if (Input.ProfileImage is not null)
        {
            var uploadResult = await _imageUploadService.UploadAsync(Input.ProfileImage, "profiles");
            if (!uploadResult.Success)
            {
                ModelState.AddModelError(nameof(Input.ProfileImage), uploadResult.ErrorMessage!);
                ProfileImagePath = user.ProfileImagePath;
                return Page();
            }
            user.ProfileImagePath = uploadResult.RelativePath;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ProfileImagePath = user.ProfileImagePath;
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated.";
        return RedirectToPage();
    }
}
