using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IRentalService
{
    Task<bool> CanRentItem(int itemId, DateTime startDate, DateTime endDate);
    Task<Rental> RequestRental(int itemId, int borrowerId, DateTime startDate, DateTime endDate);
    
    // fetching lists for requests on items owned and requests made
    Task<List<Rental>> GetIncomingRentalsAsync();
    Task<List<Rental>> GetOutgoingRentalsAsync();
    Task ApproveRental(int rentalId);
    Task RejectRental(int rentalId);
    Task ReturnRental(int rentalId);
}