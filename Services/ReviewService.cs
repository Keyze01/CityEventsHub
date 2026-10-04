using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewResult> CreateAsync(string userId, int eventId, int rating, string? comment)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null || !ev.IsPubliclyVisible)
        {
            return new ReviewResult(false, "This event is not available for review.");
        }

        if (!ev.HasEnded)
        {
            return new ReviewResult(false, "You can only review an event after it has taken place.");
        }

        if (await _context.Reviews.AnyAsync(r => r.UserId == userId && r.EventId == eventId))
        {
            return new ReviewResult(false, "You have already reviewed this event.");
        }

        _context.Reviews.Add(new Review
        {
            UserId = userId,
            EventId = eventId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return new ReviewResult(true, null);
    }

    public async Task<ReviewResult> UpdateAsync(int reviewId, string userId, int rating, string? comment)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
        if (review is null)
        {
            return new ReviewResult(false, "Review not found.");
        }

        review.Rating = rating;
        review.Comment = comment;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ReviewResult(true, null);
    }

    public async Task<bool> DeleteAsync(int reviewId, string userId)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
        if (review is null)
        {
            return false;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<Review>> GetForUserAsync(string userId)
    {
        return _context.Reviews
            .Include(r => r.Event)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
