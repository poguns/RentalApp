using Microsoft.EntityFrameworkCore;
using Moq;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.Tests.Services;

public class RentalServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RentalRepository _rentalRepository;
    private readonly ItemRepository _itemRepository;
    private readonly Mock<IAuthenticationService> _mockAuth;
    private readonly RentalService _service;

    public RentalServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        SeedData();

        _rentalRepository = new RentalRepository(_context);
        _itemRepository = new ItemRepository(_context);
        _mockAuth = new Mock<IAuthenticationService>();

        _service = new RentalService(
            _rentalRepository,
            _itemRepository,
            _mockAuth.Object
        );
    }

    private void SeedData()
    {
        _context.Users.AddRange(
            new User { Id = 1, Email = "owner@test.com", FirstName = "Owner",
                       LastName = "User", PasswordHash = "hash", PasswordSalt = "salt",
                       IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Email = "borrower@test.com", FirstName = "Borrower",
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
    public async Task CanRentItem_WithNoExistingRentals_ReturnsTrue()
    {
        // Act
        var result = await _service.CanRentItem(
            1, DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanRentItem_WithOverlappingApprovedRental_ReturnsFalse()
    {
        // Arrange
        _context.Rentals.Add(new Rental
        {
            ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            Status = RentalStatus.Approved,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CanRentItem(
            1, DateTime.Today.AddDays(2), DateTime.Today.AddDays(4));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanRentItem_WithPendingOverlap_ReturnsTrue()
    {
        // Arrange — pending rentals shouldn't block new requests
        _context.Rentals.Add(new Rental
        {
            ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            Status = RentalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CanRentItem(
            1, DateTime.Today.AddDays(2), DateTime.Today.AddDays(4));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RequestRental_WithValidDates_CreatesRental()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser)
                 .Returns(new User { Id = 2 });

        // Act
        var result = await _service.RequestRental(
            1, 2, DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RentalStatus.Pending, result.Status);
        Assert.Equal(1, result.ItemId);
        Assert.Equal(2, result.BorrowerId);
    }

    [Fact]
    public async Task RequestRental_WithEndBeforeStart_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RequestRental(
                1, 2,
                DateTime.Today.AddDays(5),  // start
                DateTime.Today.AddDays(1)   // end before start
            )
        );
    }

    [Fact]
    public async Task RequestRental_WithPastStartDate_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RequestRental(
                1, 2,
                DateTime.Today.AddDays(-1), // past date
                DateTime.Today.AddDays(3)
            )
        );
    }

    [Fact]
    public async Task ApproveRental_WithPendingRental_SetsStatusToApproved()
    {
        // Arrange
        var owner = new User { Id = 1 };
        _mockAuth.Setup(a => a.CurrentUser).Returns(owner);

        _context.Rentals.Add(new Rental
        {
            Id = 1, ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Status = RentalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.ApproveRental(1);

        // Assert
        var rental = await _context.Rentals.FindAsync(1);
        Assert.Equal(RentalStatus.Approved, rental!.Status);
    }

    [Fact]
    public async Task ApproveRental_WithAlreadyApprovedRental_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 1 });

        _context.Rentals.Add(new Rental
        {
            Id = 1, ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Status = RentalStatus.Approved, // already approved
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ApproveRental(1));
    }

    [Theory]
    [InlineData(RentalStatus.Pending, RentalStatus.Approved)]
    [InlineData(RentalStatus.Approved, RentalStatus.OutForRent)]
    [InlineData(RentalStatus.OutForRent, RentalStatus.Returned)]
    [InlineData(RentalStatus.Returned, RentalStatus.Completed)]
    public void RentalStatus_ValidTransitions_AreAllowed(
        string fromStatus, string toStatus)
    {
        // Act
        var result = RentalStatus.CanTransitionTo(fromStatus, toStatus);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(RentalStatus.Pending, RentalStatus.Completed)]
    [InlineData(RentalStatus.Rejected, RentalStatus.Approved)]
    [InlineData(RentalStatus.Completed, RentalStatus.Pending)]
    [InlineData(RentalStatus.OutForRent, RentalStatus.Approved)]
    public void RentalStatus_InvalidTransitions_AreRejected(
        string fromStatus, string toStatus)
    {
        // Act
        var result = RentalStatus.CanTransitionTo(fromStatus, toStatus);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}