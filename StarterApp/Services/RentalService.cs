public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IItemRepository _itemRepository;

    public async Task<bool> CanRentItem(int itemId, DateTime startDate, DateTime endDate)
    {
        // Check for date overlaps with existing approved rentals
        var existingRentals = await _rentalRepository.GetByItemIdAsync(itemId);
        return !existingRentals.Any(r =>
            r.Status == "Approved" &&
            r.StartDate < endDate &&
            r.EndDate > startDate);
    }
}