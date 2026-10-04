using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Admin.Users;

public class UserRow
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsLockedOut { get; set; }
}

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public List<UserRow> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserRow
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                IsLockedOut = await _userManager.IsLockedOutAsync(user)
            });
        }
    }

    public async Task<IActionResult> OnPostToggleLockAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return RedirectToPage();
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            StatusMessage = $"{user.Email} has been unlocked.";
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            StatusMessage = $"{user.Email} has been locked out.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleRoleAsync(string userId, string role)
    {
        if (role != "Organizer" && role != "Administrator")
        {
            return RedirectToPage();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return RedirectToPage();
        }

        if (await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.RemoveFromRoleAsync(user, role);
            StatusMessage = $"Removed {role} role from {user.Email}.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, role);
            StatusMessage = $"Granted {role} role to {user.Email}.";
        }

        return RedirectToPage();
    }
}
