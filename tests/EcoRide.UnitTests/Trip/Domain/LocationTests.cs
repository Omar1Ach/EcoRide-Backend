using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.UnitTests.Trip.Domain;

/// <summary>
/// Unit tests for Location value object
/// Tests GPS coordinate validation
/// </summary>
public class LocationTests
{
    [Theory]
    [InlineData(0, 0)] // Equator, Prime Meridian
    [InlineData(33.5731, -7.5898)] // Casablanca
    [InlineData(-90, -180)] // Min values
    [InlineData(90, 180)] // Max values
    [InlineData(45.5, 75.3)] // Random valid
    public void Create_WithValidCoordinates_ShouldSucceed(double latitude, double longitude)
    {
        // Act
        var result = Location.Create(latitude, longitude);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(latitude, result.Value.Latitude);
        Assert.Equal(longitude, result.Value.Longitude);
    }

    [Theory]
    [InlineData(-91, 0)] // Latitude too low
    [InlineData(91, 0)] // Latitude too high
    [InlineData(-100, 0)]
    [InlineData(100, 0)]
    public void Create_WithInvalidLatitude_ShouldFail(double latitude, double longitude)
    {
        // Act
        var result = Location.Create(latitude, longitude);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Location.InvalidLatitude", result.Error.Code);
        Assert.Contains("-90 and 90", result.Error.Message);
    }

    [Theory]
    [InlineData(0, -181)] // Longitude too low
    [InlineData(0, 181)] // Longitude too high
    [InlineData(0, -200)]
    [InlineData(0, 200)]
    public void Create_WithInvalidLongitude_ShouldFail(double latitude, double longitude)
    {
        // Act
        var result = Location.Create(latitude, longitude);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Location.InvalidLongitude", result.Error.Code);
        Assert.Contains("-180 and 180", result.Error.Message);
    }

    [Fact]
    public void ValueObject_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var location1 = Location.Create(33.5731, -7.5898).Value;
        var location2 = Location.Create(33.5731, -7.5898).Value;
        var location3 = Location.Create(33.5741, -7.5888).Value;

        // Assert
        Assert.Equal(location1, location2);
        Assert.NotEqual(location1, location3);
    }
}
