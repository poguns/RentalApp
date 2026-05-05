using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IItemRepository : IRepository<Item>
{
    //items owned by the user
    Task<List<Item>> GetByOwnerIdAsync(int ownerId);
}