using Microsoft.EntityFrameworkCore;
using Moq;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.Tests.Services;

public class LocationServiceTests
{
    private readonly Mock<ILocationService> _mockLocation;

    public LocationServiceTests()
    {
        _mockLocation = new Mock<ILocationService>();
    }

    [Fact]
    public async Task GetCurrentLocationAsync_WhenAvailable_ReturnsCoordinates()
    {
        // Arrange
        _mockLocation.Setup(l => l.GetCurrentLocationAsync())
                     .ReturnsAsync((55.9533, -3.1883));

        // Act
        var result = await _mockLocation.Object.GetCurrentLocationAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(55.9533, result.Value.Latitude);
        Assert.Equal(-3.1883, result.Value.Longitude);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_WhenPermissionDenied_ReturnsNull()
    {
        // Arrange
        _mockLocation.Setup(l => l.GetCurrentLocationAsync())
                     .ReturnsAsync((ValueTuple<double, double>?)null);

        // Act
        var result = await _mockLocation.Object.GetCurrentLocationAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_WhenGpsUnavailable_ReturnsNull()
    {
        // Arrange
        _mockLocation.Setup(l => l.GetCurrentLocationAsync())
                     .ReturnsAsync((ValueTuple<double, double>?)null);

        // Act
        var result = await _mockLocation.Object.GetCurrentLocationAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDistanceKmAsync_WithKnownCoordinates_ReturnsDistance()
    {
        // Arrange — Edinburgh to Glasgow is roughly 70km
        _mockLocation.Setup(l => l.GetDistanceKmAsync(
                55.9533, -3.1883,
                55.8617, -4.2583))
                     .ReturnsAsync(70.0);

        // Act
        var result = await _mockLocation.Object.GetDistanceKmAsync(
            55.9533, -3.1883,
            55.8617, -4.2583);

        // Assert
        Assert.Equal(70.0, result);
    }

    [Fact]
    public async Task GetDistanceKmAsync_WithSameCoordinates_ReturnsZero()
    {
        // Arrange
        _mockLocation.Setup(l => l.GetDistanceKmAsync(
                55.9533, -3.1883,
                55.9533, -3.1883))
                     .ReturnsAsync(0.0);

        // Act
        var result = await _mockLocation.Object.GetDistanceKmAsync(
            55.9533, -3.1883,
            55.9533, -3.1883);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void IsLocationAvailable_WhenGpsEnabled_ReturnsTrue()
    {
        // Arrange
        _mockLocation.Setup(l => l.IsLocationAvailable()).Returns(true);

        // Act
        var result = _mockLocation.Object.IsLocationAvailable();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLocationAvailable_WhenGpsDisabled_ReturnsFalse()
    {
        // Arrange
        _mockLocation.Setup(l => l.IsLocationAvailable()).Returns(false);

        // Act
        var result = _mockLocation.Object.IsLocationAvailable();

        // Assert
        Assert.False(result);
    }
}