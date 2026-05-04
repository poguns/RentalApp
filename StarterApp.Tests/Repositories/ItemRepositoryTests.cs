using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Tests.Fixtures;

namespace StarterApp.Tests.Repositories;

public class ItemRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly ItemRepository _repository;

    public ItemRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        // Seed a user first (items need an owner)
        _context.Users.Add(new User
        {
            Id = 1,
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        _repository = new ItemRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveItems()
    {
        // Arrange
        _context.Items.AddRange(
            new Item { Id = 1, Title = "Active Item", OwnerId = 1,
                       DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Item { Id = 2, Title = "Inactive Item", OwnerId = 1,
                       DailyRate = 5m, IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Item", result[0].Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsItem()
    {
        // Arrange
        _context.Items.Add(new Item
        {
            Id = 1, Title = "Test Item", OwnerId = 1,
            DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Item", result.Title);
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
    public async Task CreateAsync_AddsItemToDatabase()
    {
        // Arrange
        var item = new Item
        {
            Title = "New Item",
            OwnerId = 1,
            DailyRate = 10m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(item);

        // Assert
        Assert.NotEqual(0, result.Id);
        var inDb = await _context.Items.FindAsync(result.Id);
        Assert.NotNull(inDb);
        Assert.Equal("New Item", inDb.Title);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesItemInDatabase()
    {
        // Arrange
        var item = new Item
        {
            Id = 1, Title = "Original Title", OwnerId = 1,
            DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        // Act
        item.Title = "Updated Title";
        await _repository.UpdateAsync(item);

        // Assert
        var updated = await _context.Items.FindAsync(1);
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesSetsIsActiveToFalse()
    {
        // Arrange
        _context.Items.Add(new Item
        {
            Id = 1, Title = "Item to Delete", OwnerId = 1,
            DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(1);

        // Assert — record still exists but IsActive is false
        var item = await _context.Items.FindAsync(1);
        Assert.NotNull(item);
        Assert.False(item.IsActive);
    }

    [Fact]
    public async Task GetByOwnerIdAsync_ReturnsOnlyOwnerItems()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Id = 2, Email = "other@test.com", FirstName = "Other",
            LastName = "User", PasswordHash = "hash", PasswordSalt = "salt",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Items.AddRange(
            new Item { Id = 1, Title = "Owner Item", OwnerId = 1,
                       DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Item { Id = 2, Title = "Other Item", OwnerId = 2,
                       DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOwnerIdAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Owner Item", result[0].Title);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}