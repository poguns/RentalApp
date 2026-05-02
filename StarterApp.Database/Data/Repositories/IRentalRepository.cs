using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IRentalRepository : IRepository<Rental>
{
    Task<List<Rental>> GetByItemIdAsync(int itemId);
    Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId);
    Task<List<Rental>> GetByOwnerIdAsync(int ownerId);
}