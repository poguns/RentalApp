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

    [ObservableProperty]
    private Item? _item;

    [ObservableProperty]
    private bool _isOwner;

    partial void OnItemChanged(Item? value)
    {
        Title = value?.Title ?? "Item Detail";
        IsOwner = value?.OwnerId == _authService.CurrentUser?.Id;
    }

    public ItemDetailViewModel(IItemService itemService,
                               IAuthenticationService authService)
    {
        _itemService = itemService;
        _authService = authService;
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