using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using CityEventsHub.Helpers;
using CityEventsHub.Models;
using CityEventsHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Events;

/// <summary>
/// Public event details page. Guests can view; logged-in users additionally see
/// favorite/RSVP/review actions.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IFavoriteService _favoriteService;
    private readonly IRsvpService _rsvpService;
    private readonly IReviewService _reviewService;
    private readonly ApplicationDbContext _context;

    public DetailsModel(
        IEventService eventService,
        IFavoriteService favoriteService,
        IRsvpService rsvpService,
        IReviewService reviewService,
        ApplicationDbContext context)
    {
        _eventService = eventService;
        _favoriteService = favoriteService;
        _rsvpService = rsvpService;
        _reviewService = reviewService;
        _context = context;
    }

    public Event Event { get; set; } = null!;
    public bool IsOwnerOrAdmin { get; set; }
    public int AttendeeCount { get; set; }
    public List<Review> Reviews { get; set; } = new();
    public double? AverageRating { get; set; }

    public bool IsFavorited { get; set; }
    public bool HasRsvped { get; set; }
    public Review? MyReview { get; set; }
    public bool CanReview { get; set; }

    [BindProperty]
    public ReviewInputModel ReviewInput { get; set; } = new();

    public class ReviewInputModel
    {
        [Range(1, 5, ErrorMessage = "Please select a rating between 1 and 5.")]
        public int Rating { get; set; } = 5;

        [StringLength(1000)]
        public string? Comment { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var loaded = await LoadAsync(id);
        return loaded ?? Page();
    }

    public async Task<IActionResult> OnPostToggleFavoriteAsync(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        await _favoriteService.ToggleAsync(userId, id);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRsvpAsync(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var result = await _rsvpService.RegisterAsync(userId, id);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
            result.Success ? "You are registered for this event." : result.ErrorMessage;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelRsvpAsync(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var result = await _rsvpService.CancelAsync(userId, id);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
            result.Success ? "Your registration has been cancelled." : result.ErrorMessage;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSubmitReviewAsync(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var loadError = await LoadAsync(id);
            return loadError ?? Page();
        }

        var result = await _reviewService.CreateAsync(userId, id, ReviewInput.Rating, ReviewInput.Comment);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
            result.Success ? "Thank you for your review!" : result.ErrorMessage;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteReviewAsync(int id, int reviewId)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        await _reviewService.DeleteAsync(reviewId, userId);
        TempData["StatusMessage"] = "Your review has been deleted.";
        return RedirectToPage(new { id });
    }

    private async Task<IActionResult?> LoadAsync(int id)
    {
        var ev = await _eventService.GetEntityByIdAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();
        var isOwnerOrAdmin = userId == ev.OrganizerId || User.IsInRole("Administrator");

        if (!ev.IsPubliclyVisible && !isOwnerOrAdmin)
        {
            return NotFound();
        }

        Event = ev;
        IsOwnerOrAdmin = isOwnerOrAdmin;
        AttendeeCount = await _eventService.GetAttendeeCountAsync(id);

        Reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.EventId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        AverageRating = Reviews.Any() ? Reviews.Average(r => r.Rating) : null;

        if (userId is not null)
        {
            IsFavorited = await _favoriteService.IsFavoritedAsync(userId, id);
            HasRsvped = await _rsvpService.HasActiveRsvpAsync(userId, id);
            MyReview = Reviews.FirstOrDefault(r => r.UserId == userId);
            CanReview = ev.HasEnded && MyReview is null;
        }

        return null;
    }
}
