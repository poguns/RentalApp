using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Tests.Fixtures;

namespace StarterApp.Tests.Repositories;

public class ReviewRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReviewRepository _repository;

    public ReviewRepositoryTests(DatabaseFixture fixture)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        SeedData();
        _repository = new ReviewRepository(_context);
    }

    private void SeedData()
    {
        _context.Users.AddRange(
            new User { Id = 1, Email = "owner@test.com", FirstName = "Owner",
                       LastName = "User", PasswordHash = "hash", PasswordSalt = "salt",
                       IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Email = "reviewer@test.com", FirstName = "Reviewer",
                       LastName = "User", PasswordHash = "hash", PasswordSalt = "salt",
                       IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        _context.Items.Add(new Item
        {
            Id = 1, Title = "Test Item", OwnerId = 1,
            DailyRate = 10m, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllReviews()
    {
        // Arrange
        _context.Reviews.AddRange(
            new Review { Id = 1, ItemId = 1, ReviewerId = 2, Rating = 5,
                         Comment = "Great!", CreatedAt = DateTime.UtcNow },
            new Review { Id = 2, ItemId = 1, ReviewerId = 1, Rating = 3,
                         Comment = "OK", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsReviewsOrderedByCreatedAtDescending()
    {
        // Arrange
        _context.Reviews.AddRange(
            new Review { Id = 1, ItemId = 1, ReviewerId = 2, Rating = 5,
                         Comment = "First", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Review { Id = 2, ItemId = 1, ReviewerId = 1, Rating = 3,
                         Comment = "Second", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal("Second", result[0].Comment);
        Assert.Equal("First", result[1].Comment);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsReview()
    {
        // Arrange
        _context.Reviews.Add(new Review
        {
            Id = 1, ItemId = 1, ReviewerId = 2,
            Rating = 4, Comment = "Good", CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Rating);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByItemIdAsync_ReturnsOnlyReviewsForThatItem()
    {
        // Arrange
        _context.Items.Add(new Item
        {
            Id = 2, Title = "Other Item", OwnerId = 1,
            DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Reviews.AddRange(
            new Review { Id = 1, ItemId = 1, ReviewerId = 2, Rating = 5,
                         Comment = "Item 1 review", CreatedAt = DateTime.UtcNow },
            new Review { Id = 2, ItemId = 2, ReviewerId = 2, Rating = 3,
                         Comment = "Item 2 review", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByItemIdAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Item 1 review", result[0].Comment);
    }

    [Fact]
    public async Task GetByReviewerIdAsync_ReturnsOnlyReviewsByThatReviewer()
    {
        // Arrange
        _context.Reviews.AddRange(
            new Review { Id = 1, ItemId = 1, ReviewerId = 2, Rating = 5,
                         Comment = "By reviewer 2", CreatedAt = DateTime.UtcNow },
            new Review { Id = 2, ItemId = 1, ReviewerId = 1, Rating = 3,
                         Comment = "By reviewer 1", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByReviewerIdAsync(2);

        // Assert
        Assert.Single(result);
        Assert.Equal("By reviewer 2", result[0].Comment);
    }

    [Fact]
    public async Task HasUserReviewedItemAsync_WhenReviewExists_ReturnsTrue()
    {
        // Arrange
        _context.Reviews.Add(new Review
        {
            Id = 1, ItemId = 1, ReviewerId = 2,
            Rating = 5, Comment = "Great!", CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasUserReviewedItemAsync(1, 2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasUserReviewedItemAsync_WhenNoReview_ReturnsFalse()
    {
        // Act
        var result = await _repository.HasUserReviewedItemAsync(1, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAverageRatingAsync_WithMultipleReviews_ReturnsCorrectAverage()
    {
        // Arrange
        _context.Reviews.AddRange(
            new Review { Id = 1, ItemId = 1, ReviewerId = 1, Rating = 4,
                         Comment = "Good", CreatedAt = DateTime.UtcNow },
            new Review { Id = 2, ItemId = 1, ReviewerId = 2, Rating = 2,
                         Comment = "OK", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAverageRatingAsync(1);

        // Assert
        Assert.Equal(3.0, result);
    }

    [Fact]
    public async Task GetAverageRatingAsync_WithNoReviews_ReturnsZero()
    {
        // Act
        var result = await _repository.GetAverageRatingAsync(1);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CreateAsync_AddsReviewToDatabase()
    {
        // Arrange
        var review = new Review
        {
            ItemId = 1, ReviewerId = 2,
            Rating = 5, Comment = "Excellent!",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(review);

        // Assert
        Assert.NotEqual(0, result.Id);
        var inDb = await _context.Reviews.FindAsync(result.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Excellent!", inDb.Comment);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesReviewInDatabase()
    {
        // Arrange
        var review = new Review
        {
            Id = 1, ItemId = 1, ReviewerId = 2,
            Rating = 3, Comment = "Original", CreatedAt = DateTime.UtcNow
        };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Act
        review.Comment = "Updated";
        await _repository.UpdateAsync(review);

        // Assert
        var updated = await _context.Reviews.FindAsync(1);
        Assert.Equal("Updated", updated!.Comment);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_RemovesReview()
    {
        // Arrange
        _context.Reviews.Add(new Review
        {
            Id = 1, ItemId = 1, ReviewerId = 2,
            Rating = 5, Comment = "To delete", CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(1);

        // Assert
        var deleted = await _context.Reviews.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_DoesNotThrow()
    {
        // Act & Assert — should not throw
        var exception = await Record.ExceptionAsync(() => _repository.DeleteAsync(999));
        Assert.Null(exception);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}