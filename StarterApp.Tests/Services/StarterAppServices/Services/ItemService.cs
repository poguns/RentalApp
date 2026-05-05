using StarterApp.Database.Models;
using StarterApp.Database.Data.Repositories;

namespace StarterApp.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IAuthenticationService _authService;

    public ItemService(IItemRepository itemRepository, 
                            IAuthenticationService authService)
    {
        _itemRepository = itemRepository;
        _authService = authService;
    }

    public Task<List<Item>> GetItemsAsync() =>
        _itemRepository.GetAllAsync();

    public Task<Item?> GetItemAsync(int id) =>
        _itemRepository.GetByIdAsync(id);

    public Task<List<Item>> GetMyItemsAsync()
    {
        var userId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");
        return _itemRepository.GetByOwnerIdAsync(userId);
    }

    public async Task<Item> CreateItemAsync(string title, string description,
                                             decimal dailyRate, string category, 
                                             string location)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required");

        if (dailyRate <= 0)
            throw new ArgumentException("Daily rate must be greater than zero");

        var item = new Item
        {
            Title = title.Trim(),
            Description = description.Trim(),
            DailyRate = dailyRate,
            Category = category.Trim(),
            Location = location.Trim(),
            OwnerId = _authService.CurrentUser?.Id
                ?? throw new InvalidOperationException("Not authenticated"),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        return await _itemRepository.CreateAsync(item);
    }

    public async Task UpdateItemAsync(Item item)
    {
        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        if (item.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("You can only edit your own items");

        await _itemRepository.UpdateAsync(item);
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await _itemRepository.GetByIdAsync(id);
        if (item == null) return;

        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        if (item.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("You can only delete your own items");

        await _itemRepository.DeleteAsync(id);
    }
}