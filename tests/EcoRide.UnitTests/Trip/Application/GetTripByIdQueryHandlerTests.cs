using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;
using EcoRide.Modules.Trip.Application.Queries.GetTripById;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Entities;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.Repositories;
using Moq;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Unit tests for GetTripByIdQueryHandler
/// Tests trip details retrieval with authorization (US-007)
/// </summary>
public class GetTripByIdQueryHandlerTests
{
    private readonly Mock<IActiveTripRepository> _tripRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IReceiptRepository> _receiptRepositoryMock;
    private readonly GetTripByIdQueryHandler _handler;

    public GetTripByIdQueryHandlerTests()
    {
        _tripRepositoryMock = new Mock<IActiveTripRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _receiptRepositoryMock = new Mock<IReceiptRepository>();
        _handler = new GetTripByIdQueryHandler(
            _tripRepositoryMock.Object,
            _vehicleRepositoryMock.Object,
            _receiptRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidTripId_ShouldReturnTripDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        var vehicle = CreateVehicle(vehicleId, "ECO-SCTR-001", VehicleType.Scooter);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _vehicleRepositoryMock
            .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        var query = new GetTripByIdQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tripId, result.Value.TripId);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("Scooter", result.Value.VehicleType);
        Assert.Equal("ECO-SCTR-001", result.Value.VehicleCode);
        Assert.False(result.Value.HasReceipt);
    }

    [Fact]
    public async Task Handle_WithNonExistentTrip_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        var query = new GetTripByIdQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, ownerId, vehicleId);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var query = new GetTripByIdQuery(tripId, unauthorizedUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.Unauthorized", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithReceipt_ShouldIndicateReceiptExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        var vehicle = CreateVehicle(vehicleId, "ECO-SCTR-001", VehicleType.Scooter);
        var receipt = CreateReceipt(tripId, userId, vehicleId);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _vehicleRepositoryMock
            .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receipt);

        var query = new GetTripByIdQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasReceipt);
        Assert.Equal("Wallet", result.Value.PaymentMethod);
    }

    [Fact]
    public async Task Handle_WithActiveTrip_ShouldReturnActiveStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateActiveTrip(tripId, userId, vehicleId);
        var vehicle = CreateVehicle(vehicleId, "ECO-SCTR-001", VehicleType.Scooter);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _vehicleRepositoryMock
            .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        var query = new GetTripByIdQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Active", result.Value.Status);
    }

    [Fact]
    public async Task Handle_WithMissingVehicle_ShouldReturnUnknownVehicleInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _vehicleRepositoryMock
            .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        var query = new GetTripByIdQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Unknown", result.Value.VehicleType);
        Assert.Equal("N/A", result.Value.VehicleCode);
    }

    [Fact]
    public async Task Handle_WithCompletedTrip_ShouldIncludeCostBreakdown()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        var vehicle = CreateVehicle(vehicleId, "ECO-SCTR-001", VehicleType.Scooter);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _vehicleRepositoryMock
            .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        var query = new GetTripByIdQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ActiveTrip.BaseCostMAD, result.Value.BaseCostMAD);
        Assert.Equal(trip.DurationMinutes * ActiveTrip.PerMinuteRateMAD, result.Value.TimeCostMAD);
        Assert.Equal(trip.TotalCost, result.Value.TotalCostMAD);
    }

    // Helper methods
    private static ActiveTrip CreateCompletedTrip(Guid tripId, Guid userId, Guid vehicleId)
    {
        var reservationId = Guid.NewGuid();
        var startLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value;

        var tripResult = ActiveTrip.Start(
            userId: userId,
            vehicleId: vehicleId,
            reservationId: reservationId,
            startLocation: startLocation);

        var trip = tripResult.Value;

        // End the trip
        var endLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(33.5831, -7.5998).Value;
        trip.End(endLocation);

        // Use reflection to set the ID
        typeof(ActiveTrip).GetProperty("Id")!.SetValue(trip, tripId);

        return trip;
    }

    private static ActiveTrip CreateActiveTrip(Guid tripId, Guid userId, Guid vehicleId)
    {
        var reservationId = Guid.NewGuid();
        var startLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value;

        var tripResult = ActiveTrip.Start(
            userId: userId,
            vehicleId: vehicleId,
            reservationId: reservationId,
            startLocation: startLocation);

        var trip = tripResult.Value;

        // Use reflection to set the ID
        typeof(ActiveTrip).GetProperty("Id")!.SetValue(trip, tripId);

        return trip;
    }

    private static Vehicle CreateVehicle(Guid vehicleId, string code, VehicleType type)
    {
        var location = EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value;
        var batteryLevel = EcoRide.Modules.Fleet.Domain.ValueObjects.BatteryLevel.Create(85).Value;
        var vehicleResult = Vehicle.Create(code, type, batteryLevel, location);

        var vehicle = vehicleResult.Value;

        // Use reflection to set the ID
        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        return vehicle;
    }

    private static Receipt CreateReceipt(Guid tripId, Guid userId, Guid vehicleId)
    {
        var receiptResult = Receipt.Create(
            tripId: tripId,
            userId: userId,
            vehicleCode: "ECO-SCTR-001",
            tripStartTime: DateTime.UtcNow.AddHours(-1),
            tripEndTime: DateTime.UtcNow,
            durationMinutes: 18,
            distanceMeters: 1800,
            startLatitude: 33.5731,
            startLongitude: -7.5898,
            endLatitude: 33.5831,
            endLongitude: -7.5998,
            baseCost: 5.0m,
            timeCost: 27.0m,
            totalCost: 32.0m,
            paymentMethod: "Wallet",
            paymentDetails: "Paid from Wallet",
            walletBalanceBefore: 100.0m,
            walletBalanceAfter: 68.0m
        );

        return receiptResult.Value;
    }
}
