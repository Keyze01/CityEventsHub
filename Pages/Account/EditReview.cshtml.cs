using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using CityEventsHub.Helpers;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Account;

[Authorize]
public class EditReviewModel : PageModel
{
    private readonly IReviewService _reviewService;
    private readonly ApplicationDbContext _context;

    public EditReviewModel(IReviewService reviewService, ApplicationDbContext context)
    {
        _reviewService = reviewService;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string EventTitle { get; set; } = string.Empty;

    public class InputModel
    {
        public int ReviewId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review is null || review.UserId != User.GetUserId())
        {
            return NotFound();
        }

        var ev = await _context.Events.FindAsync(review.EventId);
        EventTitle = ev?.Title ?? string.Empty;

        Input = new InputModel { ReviewId = review.Id, Rating = review.Rating, Comment = review.Comment };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _reviewService.UpdateAsync(Input.ReviewId, User.GetUserId()!, Input.Rating, Input.Comment);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Review updated." : result.ErrorMessage;
        return RedirectToPage("/Account/MyReviews");
    }
}
