using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiItemService : IItemService
{
    private readonly HttpClient _httpClient;

    public ApiItemService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ApiItemResponse>>("items");
        return response?.Select(MapToItem).ToList() ?? new List<Item>();
    }

    public async Task<Item?> GetItemAsync(int id)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiItemResponse>($"items/{id}");
        return response == null ? null : MapToItem(response);
    }

    public async Task<List<Item>> GetMyItemsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ApiItemResponse>>("items/mine");
        return response?.Select(MapToItem).ToList() ?? new List<Item>();
    }

    public async Task<Item> CreateItemAsync(string title, string description,
                                             decimal dailyRate, string category,
                                             string location)
    {
        var response = await _httpClient.PostAsJsonAsync("items", new
        {
            title,
            description,
            dailyRate,
            category,
            location
        });

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ApiItemResponse>();
        return MapToItem(created!);
    }

    public async Task UpdateItemAsync(Item item)
    {
        var response = await _httpClient.PatchAsJsonAsync($"items/{item.Id}", new
        {
            title = item.Title,
            description = item.Description,
            dailyRate = item.DailyRate,
            category = item.Category,
            location = item.Location
        });

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteItemAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"items/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Maps API response shape onto your local Item model
    private static Item MapToItem(ApiItemResponse r) => new Item
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description ?? string.Empty,
        DailyRate = r.DailyRate,
        Category = r.Category ?? string.Empty,
        Location = r.Location ?? string.Empty,
        OwnerId = r.OwnerId,
        CreatedAt = r.CreatedAt,
        IsActive = true
    };

    private record ApiItemResponse(
        int Id,
        string Title,
        string? Description,
        decimal DailyRate,
        string? Category,
        string? Location,
        int OwnerId,
        DateTime CreatedAt
    );
}