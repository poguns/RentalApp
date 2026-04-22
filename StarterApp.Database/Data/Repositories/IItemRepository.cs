public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<Item> GetByIdAsync(int id);
    Task<List<Item>> GetNearbyAsync(double lat, double lon, double radiusKm);
    Task<Item> CreateAsync(Item item);
    Task UpdateAsync(Item item);
}