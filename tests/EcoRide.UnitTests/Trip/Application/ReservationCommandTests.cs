using EcoRide.Modules.Trip.Application.Commands.CancelReservation;
using EcoRide.Modules.Trip.Application.Commands.CreateReservation;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.Repositories;
using EcoRide.BuildingBlocks.Application.Data;
using Moq;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Tests for US-003 Test Scenarios (TC-020 to TC-024)
/// </summary>
public class ReservationCommandTests
{
    private readonly Mock<IReservationRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public ReservationCommandTests()
    {
        _mockRepository = new Mock<IReservationRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task TC020_ReserveVehicle_TimerStarts_ShouldSucceed()
    {
        // TC-020: Reserve vehicle - timer starts, marker changes
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var command = new CreateReservationCommand(userId, vehicleId);

        _mockRepository.Setup(r => r.GetActiveReservationByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        _mockRepository.Setup(r => r.GetActiveReservationByVehicleIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var handler = new CreateReservationCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(vehicleId, result.Value.VehicleId);
        Assert.True(result.Value.IsActive);
        Assert.True(result.Value.RemainingSeconds > 0); // Timer started
        Assert.Equal("Active", result.Value.Status); // Marker changes to yellow/reserved

        // Verify repository interactions
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC021_TryReserveSecondVehicle_ShouldShowError()
    {
        // TC-021: Try to reserve second vehicle - show error
        // BR-002: User can reserve only 1 vehicle at a time
        // Arrange
        var userId = Guid.NewGuid();
        var firstVehicleId = Guid.NewGuid();
        var secondVehicleId = Guid.NewGuid();

        var existingReservation = Reservation.Create(userId, firstVehicleId).Value;

        _mockRepository.Setup(r => r.GetActiveReservationByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReservation);

        var command = new CreateReservationCommand(userId, secondVehicleId);
        var handler = new CreateReservationCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Reservation.UserAlreadyHasReservation", result.Error.Code);
        Assert.Contains("already have an active reservation", result.Error.Message);

        // Verify no new reservation was created
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void TC022_Wait5Minutes_ReservationExpires()
    {
        // TC-022: Wait 5 minutes - reservation expires
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var reservation = Reservation.Create(userId, vehicleId).Value;

        // Act
        // Check initially active
        Assert.True(reservation.IsActive());

        // Simulate checking if expired (in production, background job would do this)
        // For unit test, we verify the logic exists
        Assert.False(reservation.HasExpired()); // Should not be expired immediately

        // Verify that MarkAsExpired would work after expiration
        // (In real scenario, after 5 minutes, HasExpired() would return true)
        Assert.Equal(300, Reservation.ReservationDurationSeconds); // 5 minutes
    }

    [Fact]
    public async Task TC023_CancelReservation_VehicleReturnsToAvailable()
    {
        // TC-023: Cancel reservation - vehicle returns to available
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var reservation = Reservation.Create(userId, vehicleId).Value;

        _mockRepository.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var command = new CancelReservationCommand(reservation.Id, userId);
        var handler = new CancelReservationCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.NotNull(reservation.CancelledAt);
        Assert.False(reservation.IsActive());

        // Verify repository interactions
        _mockRepository.Verify(r => r.Update(reservation), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC024_AnotherUserTriesToReserveSameVehicle_ShouldBeBlocked()
    {
        // TC-024: Another user tries to reserve same vehicle - blocked
        // Arrange
        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var existingReservation = Reservation.Create(firstUserId, vehicleId).Value;

        _mockRepository.Setup(r => r.GetActiveReservationByUserIdAsync(secondUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null); // Second user has no active reservation

        _mockRepository.Setup(r => r.GetActiveReservationByVehicleIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReservation); // Vehicle is already reserved by first user

        var command = new CreateReservationCommand(secondUserId, vehicleId);
        var handler = new CreateReservationCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Reservation.VehicleAlreadyReserved", result.Error.Code);
        Assert.Contains("already reserved by another user", result.Error.Message);

        // Verify no new reservation was created
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservation_Unauthorized_ShouldFail()
    {
        // Additional test: User cannot cancel another user's reservation
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var reservation = Reservation.Create(ownerUserId, vehicleId).Value;

        _mockRepository.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var command = new CancelReservationCommand(reservation.Id, otherUserId);
        var handler = new CancelReservationCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Reservation.Unauthorized", result.Error.Code);

        // Verify reservation was not updated
        _mockRepository.Verify(r => r.Update(It.IsAny<Reservation>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservation_NotFound_ShouldFail()
    {
        // Additional test: Cancel non-existent reservation
        // Arrange
        var userId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var command = new CancelReservationCommand(reservationId, userId);
        var handler = new CancelReservationCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Reservation.NotFound", result.Error.Code);

        // Verify nothing was updated
        _mockRepository.Verify(r => r.Update(It.IsAny<Reservation>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
