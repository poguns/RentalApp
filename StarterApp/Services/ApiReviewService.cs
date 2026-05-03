using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiReviewService : IReviewService
{
    private readonly HttpClient _httpClient;

    public ApiReviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Review>> GetItemReviewsAsync(int itemId)
    {
        var response = await _httpClient
            .GetFromJsonAsync<ApiReviewWrapper>($"items/{itemId}/reviews");
        return response?.Reviews?.Select(r => MapToReview(r)).ToList() ?? new List<Review>();
    }

    public async Task<double> GetAverageRatingAsync(int itemId)
    {
        var reviews = await GetItemReviewsAsync(itemId);
        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<bool> CanUserReviewItemAsync(int itemId)
    {
        try
        {
            var response = await _httpClient
                .GetFromJsonAsync<CanReviewResponse>(
                    $"items/{itemId}/can-review");
            return response?.CanReview ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Review> CreateReviewAsync(int itemId, int rating, string comment)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"items/{itemId}/reviews", new
            {
                rating,
                comment
            });

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ApiReviewResponse>();
        return MapToReview(created!);
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        var response = await _httpClient.DeleteAsync($"reviews/{reviewId}");
        response.EnsureSuccessStatusCode();
    }

    private static Review MapToReview(ApiReviewResponse r) => new Review
    {
        Id = r.Id,
        ItemId = r.ItemId,
        ReviewerId = r.ReviewerId,
        Rating = r.Rating,
        Comment = r.Comment ?? string.Empty,
        CreatedAt = r.CreatedAt,
        Reviewer = r.Reviewer == null ? null : new User
        {
            Id = r.Reviewer.Id,
            FirstName = r.Reviewer.FirstName,
            LastName = r.Reviewer.LastName
        }
    };

    private record ApiReviewResponse(
        int Id,
        int ItemId,
        int ReviewerId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        ApiUserSummary? Reviewer
    );

    private class ApiReviewWrapper
    {
        public List<ApiReviewResponse> Reviews { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    private record ApiUserSummary(int Id, string FirstName, string LastName);
    private record CanReviewResponse(bool CanReview);
}