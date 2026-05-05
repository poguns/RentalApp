using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Tests.Fixtures;

namespace StarterApp.Tests.Repositories;

public class RentalRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly AppDbContext _context;
    private readonly RentalRepository _repository;
    private readonly DatabaseFixture _fixture;

    public RentalRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        SeedData();
        _repository = new RentalRepository(_context);
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
    public async Task CreateAsync_CreatesRentalWithPendingStatus()
    {
        // Arrange
        var rental = new Rental
        {
            ItemId = 1,
            BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Status = RentalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(rental);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(RentalStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetByBorrowerIdAsync_ReturnsOnlyBorrowerRentals()
    {
        // Arrange
        _context.Rentals.AddRange(
            new Rental { Id = 1, ItemId = 1, BorrowerId = 2,
                         StartDate = DateTime.Today.AddDays(1),
                         EndDate = DateTime.Today.AddDays(3),
                         Status = RentalStatus.Pending,
                         CreatedAt = DateTime.UtcNow },
            new Rental { Id = 2, ItemId = 1, BorrowerId = 1,
                         StartDate = DateTime.Today.AddDays(5),
                         EndDate = DateTime.Today.AddDays(7),
                         Status = RentalStatus.Pending,
                         CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBorrowerIdAsync(2);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].BorrowerId);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRentalStatus()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1, ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Status = RentalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();

        // Act
        rental.Status = RentalStatus.Approved;
        await _repository.UpdateAsync(rental);

        // Assert
        var updated = await _context.Rentals.FindAsync(1);
        Assert.Equal(RentalStatus.Approved, updated!.Status);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}