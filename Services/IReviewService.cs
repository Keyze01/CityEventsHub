using CityEventsHub.Models;

namespace CityEventsHub.Services;

public record ReviewResult(bool Success, string? ErrorMessage);

public interface IReviewService
{
    Task<ReviewResult> CreateAsync(string userId, int eventId, int rating, string? comment);
    Task<ReviewResult> UpdateAsync(int reviewId, string userId, int rating, string? comment);
    Task<bool> DeleteAsync(int reviewId, string userId);
    Task<List<Review>> GetForUserAsync(string userId);
}
