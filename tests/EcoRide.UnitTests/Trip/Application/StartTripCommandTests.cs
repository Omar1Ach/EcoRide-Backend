using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;
using EcoRide.Modules.Trip.Application.Commands.StartTrip;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.Repositories;
using Moq;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Tests for US-004 Test Scenarios (TC-030 to TC-034)
/// </summary>
public class StartTripCommandTests
{
    private readonly Mock<IActiveTripRepository> _mockTripRepository;
    private readonly Mock<IReservationRepository> _mockReservationRepository;
    private readonly Mock<IVehicleRepository> _mockVehicleRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public StartTripCommandTests()
    {
        _mockTripRepository = new Mock<IActiveTripRepository>();
        _mockReservationRepository = new Mock<IReservationRepository>();
        _mockVehicleRepository = new Mock<IVehicleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task TC030_ScanCorrectQR_TripStarts()
    {
        // TC-030: Scan correct QR - trip starts
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var qrCode = "ECO-1234";

        var vehicle = Vehicle.Create(
            qrCode,
            VehicleType.Scooter,
            BatteryLevel.Create(80).Value,
            EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);
        vehicle.Reserve(); // Vehicle must be reserved

        var reservation = Reservation.Create(userId, vehicleId).Value;

        _mockVehicleRepository.Setup(r => r.GetByCodeAsync(qrCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockTripRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockTripRepository.Setup(r => r.GetActiveByVehicleIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockReservationRepository.Setup(r => r.GetActiveReservationByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var command = new StartTripCommand(userId, qrCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(vehicleId, result.Value.VehicleId);
        Assert.Equal("Active", result.Value.Status);
        Assert.Equal(5.0m, result.Value.TotalCost); // Base cost

        // Verify trip was created and vehicle status updated
        _mockTripRepository.Verify(r => r.AddAsync(It.IsAny<ActiveTrip>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockVehicleRepository.Verify(r => r.Update(vehicle), Times.Once);
        _mockReservationRepository.Verify(r => r.Update(reservation), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify vehicle status is InUse
        Assert.Equal(VehicleStatus.InUse, vehicle.Status);
        Assert.Equal(ReservationStatus.Converted, reservation.Status);
    }

    [Fact]
    public async Task TC031_ScanWrongQR_ShowError()
    {
        // TC-031: Scan wrong QR (invalid format) - show error
        // Arrange
        var userId = Guid.NewGuid();
        var invalidQRCode = "INVALID-CODE";

        var command = new StartTripCommand(userId, invalidQRCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("QRCode.InvalidFormat", result.Error.Code);

        // Verify nothing was saved
        _mockTripRepository.Verify(r => r.AddAsync(It.IsAny<ActiveTrip>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TC031_ScanNonExistentQR_ShowError()
    {
        // TC-031: Scan QR that doesn't exist in database
        // Arrange
        var userId = Guid.NewGuid();
        var qrCode = "ECO-9999";

        _mockVehicleRepository.Setup(r => r.GetByCodeAsync(qrCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var command = new StartTripCommand(userId, qrCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.VehicleNotFound", result.Error.Code);
    }

    [Fact]
    public async Task TC032_ScanQRForNonReservedVehicle_ShowError()
    {
        // TC-032: Scan QR for vehicle not matching reservation - show error
        // Arrange
        var userId = Guid.NewGuid();
        var reservedVehicleId = Guid.NewGuid();
        var scannedVehicleId = Guid.NewGuid();
        var qrCode = "ECO-1234";

        var scannedVehicle = Vehicle.Create(
            qrCode,
            VehicleType.Scooter,
            BatteryLevel.Create(80).Value,
            EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        typeof(Vehicle).GetProperty("Id")!.SetValue(scannedVehicle, scannedVehicleId);

        var reservation = Reservation.Create(userId, reservedVehicleId).Value; // Different vehicle

        _mockVehicleRepository.Setup(r => r.GetByCodeAsync(qrCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scannedVehicle);

        _mockTripRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockTripRepository.Setup(r => r.GetActiveByVehicleIdAsync(scannedVehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockReservationRepository.Setup(r => r.GetActiveReservationByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var command = new StartTripCommand(userId, qrCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.WrongVehicle", result.Error.Code);
        Assert.Contains("does not match your reservation", result.Error.Message);

        // Verify nothing was saved
        _mockTripRepository.Verify(r => r.AddAsync(It.IsAny<ActiveTrip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TC033_ManualCodeEntry_WorksSameAsScan()
    {
        // TC-033: Manual code entry - works same as scan
        // (Same logic as TC-030, just with manually entered code)
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var manualCode = "eco-5678"; // Lowercase, simulating manual entry

        var vehicle = Vehicle.Create(
            "ECO-5678",
            VehicleType.Bike,
            BatteryLevel.Create(90).Value,
            EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);
        vehicle.Reserve();

        var reservation = Reservation.Create(userId, vehicleId).Value;

        _mockVehicleRepository.Setup(r => r.GetByCodeAsync("ECO-5678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockTripRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockTripRepository.Setup(r => r.GetActiveByVehicleIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockReservationRepository.Setup(r => r.GetActiveReservationByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var command = new StartTripCommand(userId, manualCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Active", result.Value.Status);

        // Verify manual entry works same as scan
        _mockTripRepository.Verify(r => r.AddAsync(It.IsAny<ActiveTrip>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC034_NoActiveReservation_ShowError()
    {
        // TC-034: User has no active reservation - show error
        // (Simulates camera permission denied → manual entry → no reservation)
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var qrCode = "ECO-1234";

        var vehicle = Vehicle.Create(
            qrCode,
            VehicleType.Scooter,
            BatteryLevel.Create(80).Value,
            EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        _mockVehicleRepository.Setup(r => r.GetByCodeAsync(qrCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockTripRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockTripRepository.Setup(r => r.GetActiveByVehicleIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockReservationRepository.Setup(r => r.GetActiveReservationByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null); // No active reservation

        var command = new StartTripCommand(userId, qrCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NoActiveReservation", result.Error.Code);
        Assert.Contains("active reservation", result.Error.Message);

        // Verify nothing was saved
        _mockTripRepository.Verify(r => r.AddAsync(It.IsAny<ActiveTrip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UserAlreadyHasActiveTrip_ShouldFail()
    {
        // Additional test: User already has an active trip
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var qrCode = "ECO-1234";

        var vehicle = Vehicle.Create(
            qrCode,
            VehicleType.Scooter,
            BatteryLevel.Create(80).Value,
            EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        var existingTrip = ActiveTrip.Start(
            userId,
            Guid.NewGuid(), // Different vehicle
            null,
            EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        _mockVehicleRepository.Setup(r => r.GetByCodeAsync(qrCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockTripRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrip);

        var command = new StartTripCommand(userId, qrCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.UserAlreadyHasActiveTrip", result.Error.Code);
    }

    [Fact]
    public async Task VehicleAlreadyInUse_ShouldFail()
    {
        // Additional test: Vehicle is already in use by another user
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var qrCode = "ECO-1234";

        var vehicle = Vehicle.Create(
            qrCode,
            VehicleType.Scooter,
            BatteryLevel.Create(80).Value,
            EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        var existingTrip = ActiveTrip.Start(
            Guid.NewGuid(), // Different user
            vehicleId,
            null,
            EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(33.5731, -7.5898).Value).Value;

        _mockVehicleRepository.Setup(r => r.GetByCodeAsync(qrCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockTripRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        _mockTripRepository.Setup(r => r.GetActiveByVehicleIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrip);

        var command = new StartTripCommand(userId, qrCode, 33.5731, -7.5898);
        var handler = new StartTripCommandHandler(
            _mockTripRepository.Object,
            _mockReservationRepository.Object,
            _mockVehicleRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.VehicleAlreadyInUse", result.Error.Code);
    }
}
