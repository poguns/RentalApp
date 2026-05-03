using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetByItemIdAsync(int itemId);
    Task<List<Review>> GetByReviewerIdAsync(int reviewerId);
    Task<bool> HasUserReviewedItemAsync(int itemId, int reviewerId);
    Task<double> GetAverageRatingAsync(int itemId);
}