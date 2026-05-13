using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiItemService : IItemService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;

    public ApiItemService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        var response = await _httpClient.GetAsync("items");
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync();
            throw new Exception($"GetItems failed: {raw}");
        }
        var result = await _httpClient.GetFromJsonAsync<ApiItemsWrapper>("items");
        return result?.Items?.Select(r => MapToItem(r)).ToList() ?? new List<Item>();
    }

    public async Task<Item?> GetItemAsync(int id)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiItemResponse>($"items/{id}");
        return response == null ? null : MapToItem(response);
    }

    public async Task<List<Item>> GetMyItemsAsync()
    {
        var allItems = await GetItemsAsync();
        var currentUserId = _authService.CurrentUser?.Id;
        if (currentUserId == null) return new List<Item>();
        return allItems.Where(i => i.OwnerId == currentUserId).ToList();
    }

    public async Task<Item> CreateItemAsync(string title, string description,
                                             decimal dailyRate, string category,
                                             string location)
    {
        // map category string to API categoryId
        var categoryId = category switch
        {
            "Tools" => 1,
            "Garden" => 2,
            "Camping" => 3,
            "Sports" => 4,
            "Electronics" => 5,
            "Games" => 6,
            "DIY" => 7,
            "Cycling" => 8,
            "Music" => 9,
            "Outdoors" => 10,
            _ => 1
        };

        var response = await _httpClient.PostAsJsonAsync("items", new
        {
            title,
            description,
            dailyRate,
            categoryId,
            latitude = 55.9533, // default coordinates
            longitude = -3.1883
        });

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {raw}");
        }

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
            categoryId = item.Category switch
            {
                "Tools" => 1,
                "Garden" => 2,
                "Camping" => 3,
                "Sports" => 4,
                "Electronics" => 5,
                "Games" => 6,
                "DIY" => 7,
                "Cycling" => 8,
                "Music" => 9,
                "Outdoors" => 10,
                _ => 1
            },
            latitude = 55.9533, //default coordinates
            longitude = -3.1883
        });

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync();
            throw new Exception($"Update failed: {raw}");
        }
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

    private record ApiErrorResponse(string Error, string Message);

    private class ApiItemsWrapper
    {
        public List<ApiItemResponse> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}