using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rental>> GetByItemIdAsync(int itemId)
    {
        return await _context.Rentals
            .Where(r => r.ItemId == itemId)
            .Include(r => r.Borrower)
            .Include(r => r.Item)
            .ToListAsync();
    }

    public async Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId)
    {
        return await _context.Rentals
            .Where(r => r.BorrowerId == borrowerId)
            .Include(r => r.Item)
            .ThenInclude(i => i.Owner)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Rental>> GetByOwnerIdAsync(int ownerId)
    {
        return await _context.Rentals
            .Where(r => r.Item.OwnerId == ownerId)
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Rental?> GetByIdAsync(int id)
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Rental> CreateAsync(Rental rental)
    {
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();
        return rental;
    }

    public async Task UpdateAsync(Rental rental)
    {
        rental.UpdatedAt = DateTime.UtcNow;
        _context.Rentals.Update(rental);
        await _context.SaveChangesAsync();
    }
}