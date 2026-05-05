using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IItemService _itemService;

    [ObservableProperty]
    private ObservableCollection<Item> _items = new();

    public ItemsListViewModel(IItemService itemService)
    {
        _itemService = itemService;
        Title = "Items";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            var items = await _itemService.GetItemsAsync();
            Items = new ObservableCollection<Item>(items);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToItemDetailAsync(Item item)
    {
        await Shell.Current.GoToAsync(
            nameof(Views.ItemDetailPage),
            new Dictionary<string, object> { ["Item"] = item }
        );
    }

    [RelayCommand]
    private async Task GoToCreateItemAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.CreateEditItemPage));
    }
}