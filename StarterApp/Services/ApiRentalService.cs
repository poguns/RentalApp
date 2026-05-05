using System.Net.Http.Json;
using StarterApp.Database.Models;
using StarterApp.Database.States;


namespace StarterApp.Services;

public class ApiRentalService : IRentalService
{
    private readonly HttpClient _httpClient;

    public ApiRentalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Rental>> GetIncomingRentalsAsync()
    {
        var response = await _httpClient
            .GetFromJsonAsync<ApiRentalWrapper>("rentals/incoming");
        return response?.Rentals?.Select(r => MapToRental(r)).ToList() ?? new List<Rental>();
    }

    public async Task<List<Rental>> GetOutgoingRentalsAsync()
    {
        var response = await _httpClient
            .GetFromJsonAsync<ApiRentalWrapper>("rentals/outgoing");
        return response?.Rentals?.Select(MapToRental).ToList() ?? new List<Rental>();
    }

    public async Task<bool> CanRentItem(int itemId, DateTime startDate, DateTime endDate)
    {
        var response = await _httpClient.GetFromJsonAsync<AvailabilityResponse>(
            $"items/{itemId}/availability?startDate={startDate:O}&endDate={endDate:O}");
        return response?.Available ?? false;
    }

    public async Task<Rental> RequestRental(int itemId, int borrowerId,
                                             DateTime startDate, DateTime endDate)
    {
        var response = await _httpClient.PostAsJsonAsync("rentals", new
        {
            itemId, startDate, endDate
        });

        if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        throw new Exception(error?.Message ?? $"Request failed: {response.StatusCode}");
    }

    var created = await response.Content.ReadFromJsonAsync<ApiRentalResponse>();
    return MapToRental(created!);
    }

    public async Task ApproveRental(int rentalId)
    {
        var response = await _httpClient
            .PatchAsJsonAsync($"rentals/{rentalId}/status", new { status = RentalStatus.Approved });
        response.EnsureSuccessStatusCode();
    }

    public async Task RejectRental(int rentalId)
    {
        var response = await _httpClient
            .PatchAsJsonAsync($"rentals/{rentalId}/status", new { status = RentalStatus.Rejected });
        response.EnsureSuccessStatusCode();
    }

    public async Task ReturnRental(int rentalId)
    {
        var response = await _httpClient
            .PatchAsJsonAsync($"rentals/{rentalId}/status", new { status = RentalStatus.Returned });
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkOutForRentAsync(int rentalId)
    {
        var response = await _httpClient
            .PatchAsJsonAsync($"rentals/{rentalId}/status", 
                new { status = RentalStatus.OutForRent });
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteRentalAsync(int rentalId)
    {
        var response = await _httpClient
            .PatchAsJsonAsync($"rentals/{rentalId}/status",
                new { status = RentalStatus.Completed });
        response.EnsureSuccessStatusCode();
    }

    private static Rental MapToRental(ApiRentalResponse r) => new Rental
    {
        Id = r.Id,
        ItemId = r.ItemId,
        BorrowerId = r.BorrowerId,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        Item = r.Item == null ? null : new Item
        {
            Id = r.Item.Id,
            Title = r.Item.Title,
            DailyRate = r.Item.DailyRate,
            OwnerId = r.Item.OwnerId
        }
    };

    private record ApiRentalResponse(
        int Id, int ItemId, int BorrowerId,
        DateTime StartDate, DateTime EndDate,
        string Status, DateTime CreatedAt,
        ApiItemSummary? Item
    );

    private class ApiRentalWrapper
    {
        public List<ApiRentalResponse> Rentals { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    private record ApiItemSummary(int Id, string Title, decimal DailyRate, int OwnerId);
    private record AvailabilityResponse(bool Available);
    private record ApiErrorResponse(string Error, string Message);
}