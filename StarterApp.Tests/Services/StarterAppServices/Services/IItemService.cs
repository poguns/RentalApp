using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IItemService
{
    Task<List<Item>> GetItemsAsync();
    Task<Item?> GetItemAsync(int id);
    Task<List<Item>> GetMyItemsAsync();
    Task<Item> CreateItemAsync(string title, string description, 
                               decimal dailyRate, string category, string location);
    Task UpdateItemAsync(Item item);
    Task DeleteItemAsync(int id);
}