using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Item)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Review>> GetByItemIdAsync(int itemId)
    {
        return await _context.Reviews
            .Where(r => r.ItemId == itemId)
            .Include(r => r.Reviewer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByReviewerIdAsync(int reviewerId)
    {
        return await _context.Reviews
            .Where(r => r.ReviewerId == reviewerId)
            .Include(r => r.Item)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasUserReviewedItemAsync(int itemId, int reviewerId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ItemId == itemId && r.ReviewerId == reviewerId);
    }

    public async Task<double> GetAverageRatingAsync(int itemId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ItemId == itemId)
            .ToListAsync();

        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<Review> CreateAsync(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var review = await GetByIdAsync(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}