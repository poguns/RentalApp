using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class CreateEditItemViewModel : BaseViewModel
{
    private readonly IItemService _itemService;

    [ObservableProperty]
    private Item? _item;

    [ObservableProperty]
    private string _itemTitle = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _dailyRate;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    partial void OnItemChanged(Item? value)
    {
        if (value != null)
        {
            IsEditing = true;
            Title = "Edit Item";
            ItemTitle = value.Title;
            Description = value.Description;
            DailyRate = value.DailyRate;
            Category = value.Category;
            Location = value.Location;
        }
        else
        {
            IsEditing = false;
            Title = "Add Item";
        }
    }

    public CreateEditItemViewModel(IItemService itemService)
    {
        _itemService = itemService;
        Title = "Add Item";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        ClearError();

        if (string.IsNullOrWhiteSpace(ItemTitle))
        {
            SetError("Title is required");
            return;
        }

        if (DailyRate <= 0)
        {
            SetError("Daily rate must be greater than zero");
            return;
        }

        if (ItemTitle.Length < 5)
        {
            SetError("Title must be at least 5 characters");
            return;
        }

        try
        {
            IsBusy = true;

            if (IsEditing && Item != null)
            {
                Item.Title = ItemTitle;
                Item.Description = Description;
                Item.DailyRate = DailyRate;
                Item.Category = Category;
                Item.Location = Location;
                await _itemService.UpdateItemAsync(Item);
                await Shell.Current.DisplayAlert("Success", "Item updated successfully", "OK");
                await Shell.Current.GoToAsync("//ItemsListPage");
            }
            else
            {
                await _itemService.CreateItemAsync(
                    ItemTitle, Description, DailyRate, Category, Location);
                    await Shell.Current.DisplayAlert("Success", "Item created successfully", "OK");
                    await Shell.Current.GoToAsync("//ItemsListPage");
            }

            await Shell.Current.GoToAsync("//ItemsListPage");
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

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("//ItemsListPage");
    }
}