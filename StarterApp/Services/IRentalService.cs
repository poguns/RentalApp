public interface IRentalService
{
    Task<bool> CanRentItem(int itemId, DateTime startDate, DateTime endDate);
    Task<Rental> RequestRental(int itemId, int borrowerId, DateTime startDate, DateTime endDate);
    Task ApproveRental(int rentalId);
    Task RejectRental(int rentalId);
    Task ReturnRental(int rentalId);
}