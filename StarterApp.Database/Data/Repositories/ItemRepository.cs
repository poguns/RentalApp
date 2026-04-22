public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public async Task<List<Item>> GetNearbyAsync(double lat, double lon, double radiusKm)
    {
        // PostGIS spatial query abstracted here
        var point = new Point(lon, lat) { SRID = 4326 };
        return await _context.Items
            .Where(i => i.Location.Distance(point) <= radiusKm * 1000)
            .ToListAsync();
    }
}
