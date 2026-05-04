using Microsoft.EntityFrameworkCore;
using Moq;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.Tests.Services;

public class ItemServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ItemRepository _itemRepository;
    private readonly Mock<IAuthenticationService> _mockAuth;
    private readonly ItemService _service;

    public ItemServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _context.Users.Add(new User
        {
            Id = 1, Email = "test@test.com", FirstName = "Test",
            LastName = "User", PasswordHash = "hash", PasswordSalt = "salt",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        _itemRepository = new ItemRepository(_context);
        _mockAuth = new Mock<IAuthenticationService>();
        _service = new ItemService(_itemRepository, _mockAuth.Object);
    }

    [Fact]
    public async Task CreateItem_WithValidData_SetsOwnerIdToCurrentUser()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 1 });

        // Act
        var result = await _service.CreateItemAsync(
            "Drill", "A good drill", 10m, "Tools", "Edinburgh");

        // Assert
        Assert.Equal(1, result.OwnerId);
        Assert.Equal("Drill", result.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateItem_WithInvalidTitle_ThrowsArgumentException(string title)
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 1 });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateItemAsync(
                title, "description", 10m, "Tools", "Edinburgh"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateItem_WithInvalidDailyRate_ThrowsArgumentException(
        decimal rate)
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 1 });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateItemAsync(
                "Drill", "description", rate, "Tools", "Edinburgh"));
    }

    [Fact]
    public async Task UpdateItem_WhenNotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 2 });

        var item = new Item
        {
            Id = 1, Title = "Item", OwnerId = 1, // owned by user 1
            DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateItemAsync(item));
    }

    [Fact]
    public async Task GetItems_ReturnsAllActiveItems()
    {
        // Arrange
        _context.Items.AddRange(
            new Item { Id = 1, Title = "Active", OwnerId = 1,
                       DailyRate = 5m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Item { Id = 2, Title = "Inactive", OwnerId = 1,
                       DailyRate = 5m, IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result[0].Title);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}