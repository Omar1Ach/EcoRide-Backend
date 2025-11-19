using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.UnitTests.Trip.Domain;

/// <summary>
/// Tests for ActiveTrip cost calculation
/// Tests US-005 BR-004: Cost = 5 + (1.5 × minutes), rounded to nearest MAD
/// </summary>
public class ActiveTripCostTests
{
    [Fact]
    public void TC040_StartTrip_TimerBeginsAt00_00()
    {
        // TC-040: Start trip - timer begins at 00:00
        // Arrange & Act
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        // Assert
        Assert.Equal(0, trip.DurationMinutes);
        Assert.True(trip.GetCurrentDurationMinutes() >= 0); // Should be 0 or very close
    }

    [Fact]
    public void TC041_Wait1Minute_CostUpdatesTo7MAD()
    {
        // TC-041: Wait 1 minute - cost updates to 7 MAD (5 + 1.5)
        // BR-004: Cost = 5 + (1.5 × 1) = 6.5, rounded to 7 MAD
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        // Simulate 1 minute by using reflection to set start time
        // Use 1 minute minus 1 second to avoid ceiling issues from test execution time
        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-1).AddSeconds(1));

        // Act
        var cost = trip.GetCurrentEstimatedCost();

        // Assert
        Assert.Equal(7m, cost); // 5 + (1.5 × 1) = 6.5 → rounded to 7
    }

    [Fact]
    public void TC042_Wait20Minutes_CostShows35MAD()
    {
        // TC-042: Wait 20 minutes - cost shows 35 MAD (5 + 30)
        // BR-004: Cost = 5 + (1.5 × 20) = 35 MAD
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        // Simulate 20 minutes
        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-20).AddSeconds(1));

        // Act
        var cost = trip.GetCurrentEstimatedCost();

        // Assert
        Assert.Equal(35m, cost); // 5 + (1.5 × 20) = 35 MAD (no rounding needed)
    }

    [Theory]
    [InlineData(1, 7)]    // 5 + 1.5 = 6.5 → 7
    [InlineData(2, 8)]    // 5 + 3.0 = 8.0 → 8
    [InlineData(3, 10)]   // 5 + 4.5 = 9.5 → 10
    [InlineData(10, 20)]  // 5 + 15 = 20 → 20
    [InlineData(20, 35)]  // 5 + 30 = 35 → 35
    [InlineData(30, 50)]  // 5 + 45 = 50 → 50
    public void BR004_CostCalculation_ShouldRoundCorrectly(int minutes, decimal expectedCost)
    {
        // BR-004: Cost formula and rounding
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-minutes).AddSeconds(1));

        // Act
        var cost = trip.GetCurrentEstimatedCost();

        // Assert
        Assert.Equal(expectedCost, cost);
    }

    [Fact]
    public void BR004_PerMinuteRate_ShouldBe1Point5MAD()
    {
        // BR-004: Per-minute rate is 1.5 MAD
        Assert.Equal(1.5m, ActiveTrip.PerMinuteRateMAD);
    }

    [Fact]
    public void BR004_BaseCost_ShouldBe5MAD()
    {
        // BR-004: Base cost is 5 MAD
        Assert.Equal(5.0m, ActiveTrip.BaseCostMAD);
    }

    [Fact]
    public void GetMockDistance_ShouldCalculate100MetersPerMinute()
    {
        // BR-004: Mock distance +100m/minute
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-5).AddSeconds(1));

        // Act
        var distance = trip.GetMockDistanceMeters();

        // Assert
        Assert.Equal(500, distance); // 5 minutes × 100m = 500m
    }

    [Fact]
    public void GetMockDistance_At0Minutes_ShouldBe0()
    {
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        // Act
        var distance = trip.GetMockDistanceMeters();

        // Assert
        Assert.True(distance >= 0);
        Assert.True(distance <= 100); // Should be 0 or very small
    }

    [Fact]
    public void EndTrip_ShouldRoundFinalCost()
    {
        // BR-004: Final cost should also be rounded
        // Arrange
        var trip = ActiveTrip.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Location.Create(33.5731, -7.5898).Value).Value;

        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-3).AddSeconds(1)); // 3 minutes

        var endLocation = Location.Create(33.5741, -7.5888).Value;

        // Act
        trip.End(endLocation);

        // Assert
        // 3 minutes: 5 + (1.5 × 3) = 9.5 → 10 MAD
        Assert.Equal(10m, trip.TotalCost);
    }
}
