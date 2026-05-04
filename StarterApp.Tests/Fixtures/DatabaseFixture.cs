using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;

namespace StarterApp.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
        SeedData();
    }

    private void SeedData()
    {
        // Seed test users
        var user1 = new StarterApp.Database.Models.User
        {
            Id = 1,
            Email = "owner@test.com",
            FirstName = "Item",
            LastName = "Owner",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new StarterApp.Database.Models.User
        {
            Id = 2,
            Email = "borrower@test.com",
            FirstName = "Item",
            LastName = "Borrower",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Seed test items
        var item1 = new StarterApp.Database.Models.Item
        {
            Id = 1,
            Title = "Test Drill",
            Description = "A test drill",
            DailyRate = 10.00m,
            Category = "Tools",
            Location = "Edinburgh",
            OwnerId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var item2 = new StarterApp.Database.Models.Item
        {
            Id = 2,
            Title = "Test Ladder",
            Description = "A test ladder",
            DailyRate = 5.00m,
            Category = "Tools",
            Location = "Glasgow",
            OwnerId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        Context.Users.AddRange(user1, user2);
        Context.Items.AddRange(item1, item2);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}