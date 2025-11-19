using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;
using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;
using EcoRide.Modules.Trip.Application.Commands.EndTrip;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Repositories;
using Moq;
using TripLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location;
using FleetLocation = EcoRide.Modules.Fleet.Domain.ValueObjects.Location;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Tests for US-006: End Trip & Payment
/// Tests TC-050, TC-051, TC-052
/// </summary>
public class EndTripCommandTests
{
    private readonly Mock<IActiveTripRepository> _mockTripRepository;
    private readonly Mock<IVehicleRepository> _mockVehicleRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly EndTripCommandHandler _handler;

    public EndTripCommandTests()
    {
        _mockTripRepository = new Mock<IActiveTripRepository>();
        _mockVehicleRepository = new Mock<IVehicleRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new EndTripCommandHandler(
            _mockTripRepository.Object,
            _mockVehicleRepository.Object,
            _mockUserRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task TC050_EndTrip_SummaryLoadsCorrectly()
    {
        // TC-050: End trip - summary loads correctly
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var command = new EndTripCommand(userId, 33.5741, -7.5888);

        // Create trip that started 18 minutes ago
        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-18).AddSeconds(1));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        // Reserve and start trip on vehicle
        vehicle.Reserve();
        vehicle.StartTrip();

        // User with 50 MAD wallet balance
        var user = User.CreatePendingRegistration(
            Email.Create("user@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(50m);

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var summary = result.Value;

        Assert.Equal(trip.Id, summary.TripId);
        Assert.Equal(userId, summary.UserId);
        Assert.Equal(vehicleId, summary.VehicleId);
        Assert.Equal("BIKE-001", summary.VehicleCode);

        // 18 minutes duration
        Assert.Equal(18, summary.DurationMinutes);
        Assert.Equal("18 minutes", summary.DurationFormatted);

        // Distance: 18 minutes × 100m = 1800m = 1.8 km
        Assert.Equal(1800, summary.DistanceMeters);
        Assert.Equal("1.8 km", summary.DistanceFormatted);

        // Cost: 5 + (18 × 1.5) = 5 + 27 = 32 MAD
        Assert.Equal(5m, summary.BaseCost);
        Assert.Equal(27m, summary.TimeCost);
        Assert.Equal(32m, summary.TotalCost);

        // Payment status
        Assert.Equal("Paid from Wallet", summary.PaymentStatus);
        Assert.Equal(50m, summary.WalletBalanceBefore);
        Assert.Equal(18m, summary.WalletBalanceAfter); // 50 - 32 = 18

        // Verify trip was ended
        Assert.Equal(EcoRide.Modules.Trip.Domain.Enums.TripStatus.Completed, trip.Status);
        Assert.NotNull(trip.EndTime);

        // Verify vehicle made available
        _mockVehicleRepository.Verify(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()), Times.Once);

        // Verify changes were saved
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC051_WalletHas50MAD_TripCosts32MAD_Deducted()
    {
        // TC-051: Wallet has 50 MAD, trip costs 32 MAD - deducted
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var command = new EndTripCommand(userId, 33.5741, -7.5888);

        // Create trip that started 18 minutes ago (costs 32 MAD)
        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-18).AddSeconds(1));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        // Reserve and start trip on vehicle
        vehicle.Reserve();
        vehicle.StartTrip();

        // User with exactly 50 MAD wallet balance
        var user = User.CreatePendingRegistration(
            Email.Create("user@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(50m);

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var summary = result.Value;

        // Verify wallet was deducted correctly
        Assert.Equal(50m, summary.WalletBalanceBefore);
        Assert.Equal(18m, summary.WalletBalanceAfter); // 50 - 32 = 18
        Assert.Equal(32m, summary.TotalCost);
        Assert.Equal("Paid from Wallet", summary.PaymentStatus);
    }

    [Fact]
    public async Task TC052_WalletHas10MAD_TripCosts32MAD_InsufficientFunds()
    {
        // TC-052: Wallet has 10 MAD, trip costs 32 MAD - insufficient funds
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var command = new EndTripCommand(userId, 33.5741, -7.5888);

        // Create trip that started 18 minutes ago (costs 32 MAD)
        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-18).AddSeconds(1));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        // User with only 10 MAD wallet balance (insufficient)
        var user = User.CreatePendingRegistration(
            Email.Create("user@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(10m); // Only 10 MAD

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.InsufficientFunds", result.Error.Code);
        Assert.Contains("Insufficient wallet balance", result.Error.Message);
        Assert.Contains("Available: 10 MAD", result.Error.Message);
        Assert.Contains("Required: 32 MAD", result.Error.Message);

        // Verify trip was NOT completed (End was called but rolled back)
        // Verify changes were NOT saved
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndTrip_NoActiveTrip_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new EndTripCommand(userId, 33.5741, -7.5888);

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveTrip?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.NoActiveTrip", result.Error.Code);
    }

    [Fact]
    public async Task EndTrip_VehicleNotFound_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var command = new EndTripCommand(userId, 33.5741, -7.5888);

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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.VehicleNotFound", result.Error.Code);
    }

    [Fact]
    public async Task EndTrip_UserNotFound_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var command = new EndTripCommand(userId, 33.5741, -7.5888);

        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Trip.UserNotFound", result.Error.Code);
    }

    [Fact]
    public async Task EndTrip_1MinuteTrip_CalculatesCostCorrectly()
    {
        // Test single minute duration formatting
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var command = new EndTripCommand(userId, 33.5741, -7.5888);

        // Create trip that started 1 minute ago
        var trip = ActiveTrip.Start(
            userId,
            vehicleId,
            null,
            TripLocation.Create(33.5731, -7.5898).Value).Value;

        var startTimeField = typeof(ActiveTrip).GetProperty("StartTime")!;
        startTimeField.SetValue(trip, DateTime.UtcNow.AddMinutes(-1).AddSeconds(1));

        var vehicle = Vehicle.Create(
            "BIKE-001",
            VehicleType.Bike,
            BatteryLevel.Create(75).Value,
            FleetLocation.Create(33.5731, -7.5898).Value).Value;

        // Reserve and start trip on vehicle
        vehicle.Reserve();
        vehicle.StartTrip();

        var user = User.CreatePendingRegistration(
            Email.Create("user@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(50m);

        _mockTripRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _mockVehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var summary = result.Value;

        Assert.Equal(1, summary.DurationMinutes);
        Assert.Equal("1 minute", summary.DurationFormatted); // Singular

        // Cost: 5 + (1 × 1.5) = 6.5 → 7 MAD (rounded)
        Assert.Equal(7m, summary.TotalCost);
    }
}
