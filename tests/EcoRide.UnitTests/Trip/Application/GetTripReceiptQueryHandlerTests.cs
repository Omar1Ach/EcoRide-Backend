using EcoRide.Modules.Trip.Application.Queries.GetTripReceipt;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Entities;
using EcoRide.Modules.Trip.Domain.Repositories;
using Moq;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Unit tests for GetTripReceiptQueryHandler
/// Tests receipt retrieval with authorization (US-007)
/// </summary>
public class GetTripReceiptQueryHandlerTests
{
    private readonly Mock<IReceiptRepository> _receiptRepositoryMock;
    private readonly Mock<IActiveTripRepository> _tripRepositoryMock;
    private readonly GetTripReceiptQueryHandler _handler;

    public GetTripReceiptQueryHandlerTests()
    {
        _receiptRepositoryMock = new Mock<IReceiptRepository>();
        _tripRepositoryMock = new Mock<IActiveTripRepository>();
        _handler = new GetTripReceiptQueryHandler(
            _receiptRepositoryMock.Object,
            _tripRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidTripId_ShouldReturnReceipt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        var receipt = CreateReceipt(tripId, userId);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receipt);

        var query = new GetTripReceiptQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tripId, result.Value.TripId);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("Wallet", result.Value.PaymentMethod);
        Assert.Equal(32.0m, result.Value.TotalCostMAD);
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

        var query = new GetTripReceiptQuery(tripId, userId);

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

        var query = new GetTripReceiptQuery(tripId, unauthorizedUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Receipt.Unauthorized", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithNonExistentReceipt_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        var query = new GetTripReceiptQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Receipt.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_ShouldReturnCompleteReceiptDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        var receipt = CreateReceipt(tripId, userId);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _receiptRepositoryMock
            .Setup(x => x.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receipt);

        var query = new GetTripReceiptQuery(tripId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.ReceiptNumber);
        Assert.StartsWith("RCP-", result.Value.ReceiptNumber);
        Assert.Equal("ECO-SCTR-001", result.Value.VehicleCode);
        Assert.Equal(18, result.Value.DurationMinutes);
        Assert.Equal(1800, result.Value.DistanceMeters);
        Assert.Equal(5.0m, result.Value.BaseCostMAD);
        Assert.Equal(27.0m, result.Value.TimeCostMAD);
        Assert.Equal(100.0m, result.Value.WalletBalanceBefore);
        Assert.Equal(68.0m, result.Value.WalletBalanceAfter);
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

    private static Receipt CreateReceipt(Guid tripId, Guid userId)
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
