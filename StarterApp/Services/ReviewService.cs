using StarterApp.Database.Models;
using StarterApp.Database.Data.Repositories;

namespace StarterApp.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IAuthenticationService _authService;

    public ReviewService(IReviewRepository reviewRepository, IRentalRepository rentalRepository, IAuthenticationService authService)
    {
        _reviewRepository = reviewRepository;
        _rentalRepository = rentalRepository;
        _authService = authService;
    }

    public Task<List<Review>> GetItemReviewsAsync(int itemId) =>
        _reviewRepository.GetByItemIdAsync(itemId);

    public Task<double> GetAverageRatingAsync(int itemId) =>
        _reviewRepository.GetAverageRatingAsync(itemId);

    public async Task<bool> CanUserReviewItemAsync(int itemId)
    {
        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        // check if user hasnt already reveiwed the item
        var alreadyReviewed = await _reviewRepository
            .HasUserReviewedItemAsync(itemId, currentUserId);

        if (alreadyReviewed)
            return false;

        //check if user rented the item
        var rentals = await _rentalRepository.GetByBorrowerIdAsync(currentUserId);
        var hasRentedItem = rentals.Any(r =>
            r.ItemId == itemId &&
            r.Status == RentalStatus.Completed);

        return hasRentedItem;
    }

    public async Task<Review> CreateReviewAsync(int itemId, int rating, string comment)
    {
        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        //rating range
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        //check if user can review
        var canReview = await CanUserReviewItemAsync(itemId);
        if (!canReview)
            throw new InvalidOperationException(
                "You can only review items you have rented and returned");

        var review = new Review
        {
            ItemId = itemId,
            ReviewerId = currentUserId,
            Rating = rating,
            Comment = comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return await _reviewRepository.CreateAsync(review);
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new InvalidOperationException("Review not found");

        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        //only the reviewer can delete their own review
        if (review.ReviewerId != currentUserId)
            throw new UnauthorizedAccessException(
                "You can only delete your own reviews");

        await _reviewRepository.DeleteAsync(reviewId);
    }
}