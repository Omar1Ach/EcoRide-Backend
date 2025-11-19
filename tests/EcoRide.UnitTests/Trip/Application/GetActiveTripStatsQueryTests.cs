using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;
using EcoRide.Modules.Trip.Application.Queries.GetActiveTripStats;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Repositories;
using Moq;
using TripLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location;
using FleetLocation = EcoRide.Modules.Fleet.Domain.ValueObjects.Location;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Tests for GetActiveTripStatsQueryHandler
/// Tests US-005 real-time statistics
/// </summary>
public class GetActiveTripStatsQueryTests
{
    private readonly Mock<IActiveTripRepository> _mockTripRepository;
    private readonly Mock<IVehicleRepository> _mockVehicleRepository;
    private readonly GetActiveTripStatsQueryHandler _handler;

    public GetActiveTripStatsQueryTests()
    {
        _mockTripRepository = new Mock<IActiveTripRepository>();
        _mockVehicleRepository = new Mock<IVehicleRepository>();
        _handler = new GetActiveTripStatsQueryHandler(_mockTripRepository.Object, _mockVehicleRepository.Object);
    }

    [Fact]
    public async Task Handle_NoActiveTrip_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NoActiveTrip", result.Error.Code);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.VehicleNotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_ActiveTrip_ReturnsStatsWithFormattedDuration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        // Set trip to 5 minutes ago
        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-5).AddSeconds(-30));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        // Duration should be around 5:30 (5 minutes 30 seconds)
        Assert.True(stats.DurationSeconds >= 330 && stats.DurationSeconds <= 332); // Allow 2 second variance
        Assert.Matches(@"^\d{2}:\d{2}$", stats.DurationFormatted); // MM:SS format
        Assert.Contains("05:", stats.DurationFormatted); // Should show 05:XX
    }

    [Fact]
    public async Task Handle_ActiveTrip_CalculatesCorrectCost()
    {
        // BR-004: Cost = 5 + (1.5 × minutes), rounded
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        // Set trip to 3 minutes ago
        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-3).AddSeconds(1));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        // 3 minutes: 5 + (1.5 × 3) = 9.5 → 10 MAD
        Assert.Equal(10m, stats.CurrentCost);
        Assert.Equal(5.0m, stats.BaseCost);
        Assert.Equal(1.5m, stats.PerMinuteRate);
    }

    [Fact]
    public async Task Handle_ActiveTrip_FormatsDistanceInMeters()
    {
        // Mock distance: +100m/minute
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        // Set trip to 5 minutes ago
        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-5).AddSeconds(1));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(500, stats.DistanceMeters); // 5 minutes × 100m = 500m
        Assert.Equal("500 m", stats.DistanceFormatted);
    }

    [Fact]
    public async Task Handle_ActiveTrip_FormatsDistanceInKilometers()
    {
        // Distance >= 1000m should be shown in km
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        // Set trip to 15 minutes ago
        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-15).AddSeconds(1));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(1500, stats.DistanceMeters); // 15 minutes × 100m = 1500m
        Assert.Equal("1.5 km", stats.DistanceFormatted);
    }

    [Fact]
    public async Task TC044_BatteryBelow10Percent_ShowsWarning()
    {
        // TC-044: Battery drops below 10% - show warning
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(8).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value; // Low battery: 8%

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(8, stats.BatteryPercentage);
        Assert.True(stats.IsLowBattery); // Should show warning
    }

    [Fact]
    public async Task TC044_BatteryAt10PercentOrAbove_NoWarning()
    {
        // TC-044: Battery at 10% or above - no warning
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(10).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value; // Exactly 10%

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(10, stats.BatteryPercentage);
        Assert.False(stats.IsLowBattery); // Should NOT show warning
    }

    [Fact]
    public async Task Handle_ReturnsCorrectTripMetadata()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var query = new GetActiveTripStatsQuery(userId);

        var startLocation = TripLocation.Create(33.5731, -7.5898).Value;
        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            startLocation).Value;

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(trip.Id, stats.TripId);
        Assert.Equal(userId, stats.UserId);
        Assert.Equal(vehicleId, stats.VehicleId);
        Assert.Equal("BIKE-001", stats.VehicleCode);
        Assert.Equal(33.5731, stats.StartLatitude);
        Assert.Equal(-7.5898, stats.StartLongitude);
    }
}
