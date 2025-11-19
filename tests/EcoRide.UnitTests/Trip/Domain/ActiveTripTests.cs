using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.UnitTests.Trip.Domain;

/// <summary>
/// Unit tests for ActiveTrip aggregate
/// Tests US-004 business rules (BR-003)
/// </summary>
public class ActiveTripTests
{
    [Fact]
    public void Start_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var location = Location.Create(33.5731, -7.5898).Value;

        // Act
        var result = ActiveTrip.Start(userId, vehicleId, reservationId, location);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(vehicleId, result.Value.VehicleId);
        Assert.Equal(reservationId, result.Value.ReservationId);
        Assert.Equal(TripStatus.Active, result.Value.Status);
        Assert.Equal(ActiveTrip.BaseCostMAD, result.Value.TotalCost);
        Assert.True(result.Value.IsActive());
    }

    [Fact]
    public void Start_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var userId = Guid.Empty;
        var vehicleId = Guid.NewGuid();
        var location = Location.Create(33.5731, -7.5898).Value;

        // Act
        var result = ActiveTrip.Start(userId, vehicleId, null, location);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.InvalidUserId", result.Error.Code);
    }

    [Fact]
    public void Start_WithEmptyVehicleId_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.Empty;
        var location = Location.Create(33.5731, -7.5898).Value;

        // Act
        var result = ActiveTrip.Start(userId, vehicleId, null, location);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.InvalidVehicleId", result.Error.Code);
    }

    [Fact]
    public void Start_BR003_BaseCost_ShouldBe5MAD()
    {
        // BR-003: Base cost is 5 MAD
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var location = Location.Create(33.5731, -7.5898).Value;

        // Act
        var result = ActiveTrip.Start(userId, vehicleId, null, location);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5.0m, result.Value.TotalCost);
        Assert.Equal(5.0m, ActiveTrip.BaseCostMAD);
    }

    [Fact]
    public void End_WhenActive_ShouldCalculateCost()
    {
        // BR-003: Cost = 5 MAD + (minutes * 1 MAD/min)
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        Thread.Sleep(2000); // Wait 2 seconds to simulate trip duration
        var endLocation = Location.Create(33.5741, -7.5888).Value;

        // Act
        var result = trip.End(endLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TripStatus.Completed, trip.Status);
        Assert.NotNull(trip.EndTime);
        Assert.True(trip.DurationMinutes >= 0);
        Assert.True(trip.TotalCost >= ActiveTrip.BaseCostMAD); // At least base cost
        Assert.False(trip.IsActive());
    }

    [Fact]
    public void End_WhenNotActive_ShouldFail()
    {
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        var endLocation = Location.Create(33.5741, -7.5888).Value;
        trip.End(endLocation); // End once

        // Act
        var result = trip.End(endLocation); // Try to end again

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NotActive", result.Error.Code);
    }

    [Fact]
    public void Cancel_WhenActive_ShouldSucceed()
    {
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        // Act
        var result = trip.Cancel();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TripStatus.Cancelled, trip.Status);
        Assert.NotNull(trip.EndTime);
        Assert.False(trip.IsActive());
    }

    [Fact]
    public void Cancel_WhenNotActive_ShouldFail()
    {
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        trip.Cancel(); // Cancel once

        // Act
        var result = trip.Cancel(); // Try to cancel again

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NotActive", result.Error.Code);
    }

    [Fact]
    public void GetCurrentDurationMinutes_WhenActive_ShouldReturnDuration()
    {
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        Thread.Sleep(1000); // Wait 1 second

        // Act
        var duration = trip.GetCurrentDurationMinutes();

        // Assert
        Assert.True(duration >= 0);
    }

    [Fact]
    public void GetCurrentEstimatedCost_WhenActive_ShouldCalculateCorrectly()
    {
        // BR-003: Cost = 5 MAD + (minutes * 1 MAD/min)
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        // Act
        var cost = trip.GetCurrentEstimatedCost();

        // Assert
        Assert.True(cost >= ActiveTrip.BaseCostMAD);
    }
}
