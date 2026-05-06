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

    [ObservableProperty]
    private ObservableCollection<Item> _myItems = new();

    [ObservableProperty]
    private bool _showMyItems = false;


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
            var allItems = await _itemService.GetItemsAsync();
            var myItems = await _itemService.GetMyItemsAsync();

            var myItemIds = myItems.Select(i => i.Id).ToHashSet();

            // remove own items from all items list
            var otherItems = allItems.Where(i => !myItemIds.Contains(i.Id)).ToList();

            Items = new ObservableCollection<Item>(otherItems);
            MyItems = new ObservableCollection<Item>(myItems);
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

    [RelayCommand]
    private void ToggleMyItems()
    {
        ShowMyItems = !ShowMyItems;
    }

    [RelayCommand]
    private async Task LoadMyItemsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ClearError();
            var items = await _itemService.GetMyItemsAsync();
            var myItems = await _itemService.GetMyItemsAsync();

            MyItems = new ObservableCollection<Item>(items);
            MyItems = new ObservableCollection<Item>(myItems);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load your items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}