using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IReviewService
{
    Task<List<Review>> GetItemReviewsAsync(int itemId);
    Task<double> GetAverageRatingAsync(int itemId);
    Task<bool> CanUserReviewItemAsync(int itemId);
    Task<Review> CreateReviewAsync(int itemId, int rating, string comment);
    Task DeleteReviewAsync(int reviewId);
}