using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;

    [ObservableProperty]
    private ObservableCollection<Rental> _incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<Rental> _outgoingRentals = new();

    [ObservableProperty]
    private bool _showingIncoming = true;

    //controls which list is visible in the UI
    [ObservableProperty]
    private ObservableCollection<Rental> _currentRentals = new();

    public RentalsViewModel(IRentalService rentalService)
    {
        _rentalService = rentalService;
        Title = "Rentals";
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            var incoming = await _rentalService.GetIncomingRentalsAsync();
            var outgoing = await _rentalService.GetOutgoingRentalsAsync();

            IncomingRentals = new ObservableCollection<Rental>(incoming);
            OutgoingRentals = new ObservableCollection<Rental>(outgoing);

            //refresh whichever list is currently showing
            UpdateCurrentRentals();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowIncoming()
    {
        ShowingIncoming = true;
        UpdateCurrentRentals();
    }

    [RelayCommand]
    private void ShowOutgoing()
    {
        ShowingIncoming = false;
        UpdateCurrentRentals();
    }

    //switches CurrentRentals to whichever list is active
    private void UpdateCurrentRentals()
    {
        CurrentRentals = ShowingIncoming ? IncomingRentals : OutgoingRentals;
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(Rental rental)
    {
        try
        {
            IsBusy = true;
            await _rentalService.ApproveRental(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to approve rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RejectRentalAsync(Rental rental)
    {
        try
        {
            IsBusy = true;
            await _rentalService.RejectRental(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to reject rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReturnRentalAsync(Rental rental)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Return Item",
            $"Confirm return of '{rental.Item?.Title}'?",
            "Confirm",
            "Cancel"
        );

        if (!confirm) return;

        try
        {
            IsBusy = true;
            await _rentalService.ReturnRental(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to return rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MarkOutForRentAsync(Rental rental)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Mark as Out for Rent",
            $"Confirm '{rental.Item?.Title}' has been collected?",
            "Confirm",
            "Cancel"
        );

        if (!confirm) return;

        try
        {
            IsBusy = true;
            await _rentalService.MarkOutForRentAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to update rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CompleteRentalAsync(Rental rental)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Complete Rental",
            $"Mark '{rental.Item?.Title}' rental as completed?",
            "Confirm",
            "Cancel"
        );

        if (!confirm) return;

        try
        {
            IsBusy = true;
            await _rentalService.CompleteRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to complete rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}