using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Application.Commands.RateTrip;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.Repositories;
using Moq;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Unit tests for RateTripCommand and handler
/// Tests trip rating feature (US-006)
/// </summary>
public class RateTripCommandTests
{
    private readonly Mock<IActiveTripRepository> _tripRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RateTripCommandHandler _handler;

    public RateTripCommandTests()
    {
        _tripRepositoryMock = new Mock<IActiveTripRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RateTripCommandHandler(
            _tripRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: userId,
            Stars: 5,
            Comment: "Great trip!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, trip.RatingStars);
        Assert.Equal("Great trip!", trip.RatingComment);
        Assert.NotNull(trip.RatedAt);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTrip_ShouldFail()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: Guid.NewGuid(),
            Stars: 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NotFound", result.Error.Code);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, ownerId, vehicleId);
        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: otherUserId, // Different user
            Stars: 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.Unauthorized", result.Error.Code);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)] // Too low
    [InlineData(6)] // Too high
    [InlineData(-1)] // Negative
    public async Task Handle_WithInvalidStars_ShouldFail(int stars)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: userId,
            Stars: stars);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Rating.InvalidStars", result.Error.Code);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithTooLongComment_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: userId,
            Stars: 5,
            Comment: new string('a', 501)); // 501 characters

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Rating.CommentTooLong", result.Error.Code);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyRatedTrip_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);

        // Rate the trip first
        var firstRating = EcoRide.Modules.Trip.Domain.ValueObjects.Rating.Create(4, "Good").Value;
        trip.AddRating(firstRating);

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: userId,
            Stars: 5,
            Comment: "Trying to rate again");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.AlreadyRated", result.Error.Code);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInProgressTrip_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        var startLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value;
        var tripResult = ActiveTrip.Start(
            userId: userId,
            vehicleId: vehicleId,
            reservationId: reservationId,
            startLocation: startLocation);

        var trip = tripResult.Value;

        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: userId,
            Stars: 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NotCompleted", result.Error.Code);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullComment_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var trip = CreateCompletedTrip(tripId, userId, vehicleId);
        _tripRepositoryMock
            .Setup(x => x.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var command = new RateTripCommand(
            TripId: tripId,
            UserId: userId,
            Stars: 4,
            Comment: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(4, trip.RatingStars);
        Assert.Null(trip.RatingComment);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

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

        // Complete the trip
        var endLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(33.5831, -7.5998).Value;
        trip.End(endLocation);

        return trip;
    }
}
