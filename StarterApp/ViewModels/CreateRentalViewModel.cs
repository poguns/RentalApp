using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class CreateRentalViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private Item? _item;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(2);

    //calculated display values, updates when dates change
    [ObservableProperty]
    private int _totalDays;

    [ObservableProperty]
    private decimal _totalCost;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private string _availabilityMessage = string.Empty;

    partial void OnItemChanged(Item? value)
    {
        Title = value != null ? $"Rent {value.Title}" : "Rent Item";
        UpdateCostPreview();
    }

    partial void OnStartDateChanged(DateTime value) => UpdateCostPreview();
    partial void OnEndDateChanged(DateTime value) => UpdateCostPreview();

    private void UpdateCostPreview()
    {
        if (Item == null || EndDate <= StartDate)
        {
            TotalDays = 0;
            TotalCost = 0;
            return;
        }

        TotalDays = (EndDate - StartDate).Days;
        TotalCost = TotalDays * Item.DailyRate;
    }

    public CreateRentalViewModel(IRentalService rentalService,
                                  IAuthenticationService authService)
    {
        _rentalService = rentalService;
        _authService = authService;
        Title = "Rent Item";
    }

    [RelayCommand]
    private async Task CheckAvailabilityAsync()
    {
        if (Item == null) return;

        try
        {
            IsBusy = true;
            ClearError();

            var available = await _rentalService.CanRentItem(
                Item.Id, StartDate, EndDate);

            IsAvailable = available;
            AvailabilityMessage = available
                ? "✓ Item is available for these dates"
                : "✗ Item is not available for these dates";
        }
        catch (Exception ex)
        {
            SetError($"Failed to check availability: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitRequestAsync()
    {
        if (Item == null) return;

        if (StartDate >= EndDate)
        {
            SetError("End date must be after start date");
            return;
        }

        if (StartDate < DateTime.Today)
        {
            SetError("Start date cannot be in the past");
            return;
        }

        var currentUserId = _authService.CurrentUser?.Id;
        if (currentUserId == null)
        {
            SetError("You must be logged in to request a rental");
            return;
        }

        // Warn if availability hasn't been checked
        if (!IsAvailable)
        {
            var proceed = await Shell.Current.DisplayAlert(
                "Warning",
                "You haven't confirmed availability. Submit anyway?",
                "Submit",
                "Cancel"
            );
            if (!proceed) return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            await _rentalService.RequestRental(
                Item.Id, currentUserId.Value, StartDate, EndDate);

            await Shell.Current.DisplayAlert(
                "Success",
                "Rental request submitted successfully",
                "OK"
            );

            await Shell.Current.GoToAsync("//ItemsListPage");
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit request: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("//ItemsListPage");
    }
}