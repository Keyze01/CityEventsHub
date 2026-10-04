using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages;

/// <summary>
/// Sets the current UI culture cookie and redirects back to the referring page.
/// Supports the English / Somali / Arabic language switcher in the site footer.
/// </summary>
public class SetLanguageModel : PageModel
{
    private static readonly string[] SupportedCultures = { "en", "so", "ar" };

    public IActionResult OnGet(string culture, string? returnUrl)
    {
        if (SupportedCultures.Contains(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        }

        return LocalRedirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}
