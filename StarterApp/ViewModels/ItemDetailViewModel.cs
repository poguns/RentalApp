using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.Views;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly IAuthenticationService _authService;
    private readonly IReviewService _reviewService;

    [ObservableProperty]
    private Item? _item;

    [ObservableProperty]
    private bool _isOwner;
    [ObservableProperty]
    private double _averageRating;

    [ObservableProperty]
    private string _reviewCount = string.Empty;

    partial void OnItemChanged(Item? value)
    {
        Title = value?.Title ?? "Item Detail";
        IsOwner = value?.OwnerId == _authService.CurrentUser?.Id;
        _ = LoadReviewSummaryAsync();
    }

    public ItemDetailViewModel(IItemService itemService, IAuthenticationService authService, IReviewService reviewService)
    {
        _itemService = itemService;
        _authService = authService;
        _reviewService = reviewService;
    }

    [RelayCommand]
    private async Task GoToEditAsync()
    {
        await Shell.Current.GoToAsync(
            nameof(Views.CreateEditItemPage),
            new Dictionary<string, object> { ["Item"] = Item! }
        );
    }

    [RelayCommand]
    private async Task GoToRentItemAsync()
    {
        await Shell.Current.GoToAsync(
            nameof(Views.CreateRentalPage),
            new Dictionary<string, object> { ["Item"] = Item! }
        );
    }

    private async Task LoadReviewSummaryAsync()
    {
        if (Item == null) return;
        AverageRating = await _reviewService.GetAverageRatingAsync(Item.Id);
    }

    [RelayCommand]
    private async Task GoToReviewsAsync()
    {
        await Shell.Current.GoToAsync(
            nameof(Views.ReviewsPage),
            new Dictionary<string, object> { ["Item"] = Item! }
        );
    }

    [RelayCommand]
    private async Task DeleteItemAsync()
    {
        if (Item == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete Item",
            $"Are you sure you want to delete '{Item.Title}'?",
            "Delete",
            "Cancel"
        );

        if (!confirm) return;

        try
        {
            IsBusy = true;
            ClearError();
            await _itemService.DeleteItemAsync(Item.Id);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}