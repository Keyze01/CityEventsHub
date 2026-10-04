using CityEventsHub.Helpers;
using CityEventsHub.Models;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Account;

[Authorize]
public class MyReviewsModel : PageModel
{
    private readonly IReviewService _reviewService;

    public MyReviewsModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public List<Review> Reviews { get; set; } = new();

    public async Task OnGetAsync()
    {
        Reviews = await _reviewService.GetForUserAsync(User.GetUserId()!);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int reviewId)
    {
        await _reviewService.DeleteAsync(reviewId, User.GetUserId()!);
        return RedirectToPage();
    }
}
