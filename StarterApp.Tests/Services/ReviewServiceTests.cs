using Microsoft.EntityFrameworkCore;
using Moq;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.Tests.Services;

public class ReviewServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReviewRepository _reviewRepository;
    private readonly RentalRepository _rentalRepository;
    private readonly Mock<IAuthenticationService> _mockAuth;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        SeedData();

        _reviewRepository = new ReviewRepository(_context);
        _rentalRepository = new RentalRepository(_context);
        _mockAuth = new Mock<IAuthenticationService>();

        _service = new ReviewService(
            _reviewRepository,
            _rentalRepository,
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
    public async Task CanUserReviewItem_WithCompletedRental_ReturnsTrue()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 2 });
        _context.Rentals.Add(new Rental
        {
            ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(-5),
            EndDate = DateTime.Today.AddDays(-1),
            Status = RentalStatus.Completed,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CanUserReviewItemAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserReviewItem_WithNoCompletedRental_ReturnsFalse()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 2 });

        // Act — no rentals seeded
        var result = await _service.CanUserReviewItemAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanUserReviewItem_WhenAlreadyReviewed_ReturnsFalse()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 2 });

        _context.Rentals.Add(new Rental
        {
            ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(-5),
            EndDate = DateTime.Today.AddDays(-1),
            Status = RentalStatus.Completed,
            CreatedAt = DateTime.UtcNow
        });
        _context.Reviews.Add(new Review
        {
            ItemId = 1, ReviewerId = 2,
            Rating = 5, Comment = "Great!",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CanUserReviewItemAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateReview_WithValidData_CreatesReview()
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 2 });
        _context.Rentals.Add(new Rental
        {
            ItemId = 1, BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(-5),
            EndDate = DateTime.Today.AddDays(-1),
            Status = RentalStatus.Completed,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CreateReviewAsync(1, 5, "Excellent item!");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        Assert.Equal("Excellent item!", result.Comment);
        Assert.Equal(2, result.ReviewerId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task CreateReview_WithInvalidRating_ThrowsArgumentException(int rating)
    {
        // Arrange
        _mockAuth.Setup(a => a.CurrentUser).Returns(new User { Id = 2 });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateReviewAsync(1, rating, "comment"));
    }

    [Fact]
    public async Task GetAverageRating_WithMultipleReviews_ReturnsCorrectAverage()
    {
        // Arrange
        _context.Reviews.AddRange(
            new Review { ItemId = 1, ReviewerId = 1, Rating = 4,
                         Comment = "Good", CreatedAt = DateTime.UtcNow },
            new Review { ItemId = 1, ReviewerId = 2, Rating = 2,
                         Comment = "OK", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAverageRatingAsync(1);

        // Assert
        Assert.Equal(3.0, result);
    }

    [Fact]
    public async Task GetAverageRating_WithNoReviews_ReturnsZero()
    {
        // Act
        var result = await _service.GetAverageRatingAsync(1);

        // Assert
        Assert.Equal(0, result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}