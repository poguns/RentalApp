using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IRentalRepository : IRepository<Rental>
{
    
    Task<List<Rental>> GetByItemIdAsync(int itemId);
    Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId);
    //rentals on items that belong to a user
    Task<List<Rental>> GetByOwnerIdAsync(int ownerId);
}