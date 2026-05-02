using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IRentalRepository
{
    Task<List<Rental>> GetByItemIdAsync(int itemId);
    Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId);
    Task<List<Rental>> GetByOwnerIdAsync(int ownerId);
    Task<Rental?> GetByIdAsync(int id);
    Task<Rental> CreateAsync(Rental rental);
    Task UpdateAsync(Rental rental);
}