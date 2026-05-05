using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class ReviewsViewModel : BaseViewModel
{
    private readonly IReviewService _reviewService;

    [ObservableProperty]
    private Item? _item;

    [ObservableProperty]
    private ObservableCollection<Review> _reviews = new();

    [ObservableProperty]
    private double _averageRating;

    [ObservableProperty]
    private bool _canReview;

    [ObservableProperty]
    private bool _showReviewForm;

    [ObservableProperty]
    private int _selectedRating = 5;

    [ObservableProperty]
    private string _comment = string.Empty;

    [ObservableProperty]
    private string _reviewCount = string.Empty;

    partial void OnItemChanged(Item? value)
    {
        if (value != null)
        {
            Title = $"Reviews — {value.Title}";
            _ = LoadReviewsAsync();
        }
    }

    public ReviewsViewModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
        Title = "Reviews";
    }

    [RelayCommand]
    private async Task LoadReviewsAsync()
    {
        if (Item == null) return;

        try
        {
            IsBusy = true;
            ClearError();

            var reviews = await _reviewService.GetItemReviewsAsync(Item.Id);
            var average = await _reviewService.GetAverageRatingAsync(Item.Id);
            var canReview = await _reviewService.CanUserReviewItemAsync(Item.Id);

            Reviews = new ObservableCollection<Review>(reviews);
            AverageRating = average;
            CanReview = canReview;
            ReviewCount = reviews.Count == 1
                ? "1 review"
                : $"{reviews.Count} reviews";
        }
        catch (Exception ex)
        {
            SetError($"Failed to load reviews: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleReviewForm()
    {
        ShowReviewForm = !ShowReviewForm;
    }

    [RelayCommand]
    private void SetRating(int rating)
    {
        SelectedRating = rating;
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (Item == null) return;

        if (SelectedRating < 1 || SelectedRating > 5)
        {
            SetError("Please select a rating between 1 and 5");
            return;
        }

        if (string.IsNullOrWhiteSpace(Comment))
        {
            SetError("Please write a comment");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            await _reviewService.CreateReviewAsync(Item.Id, SelectedRating, Comment);

            // Reset form
            Comment = string.Empty;
            SelectedRating = 5;
            ShowReviewForm = false;

            // Reload to show new review
            await LoadReviewsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit review: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteReviewAsync(Review review)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Delete Review",
            "Are you sure you want to delete this review?",
            "Delete",
            "Cancel"
        );

        if (!confirm) return;

        try
        {
            IsBusy = true;
            ClearError();

            await _reviewService.DeleteReviewAsync(review.Id);
            await LoadReviewsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete review: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}