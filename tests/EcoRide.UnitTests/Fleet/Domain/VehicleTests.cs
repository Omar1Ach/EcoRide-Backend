using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.ValueObjects;

namespace EcoRide.UnitTests.Fleet.Domain;

public class VehicleTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var code = "BIKE-001";
        var type = VehicleType.Bike;
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value; // Rabat

        // Act
        var result = Vehicle.Create(code, type, batteryLevel, location);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(code, result.Value.Code);
        Assert.Equal(type, result.Value.Type);
        Assert.Equal(batteryLevel, result.Value.BatteryLevel);
        Assert.Equal(location, result.Value.Location);
        Assert.Equal(VehicleStatus.Available, result.Value.Status);
    }

    [Fact]
    public void Create_WithEmptyCode_ShouldFail()
    {
        // Arrange
        var code = "";
        var type = VehicleType.Scooter;
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;

        // Act
        var result = Vehicle.Create(code, type, batteryLevel, location);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.CodeEmpty", result.Error.Code);
    }

    [Fact]
    public void IsAvailableForReservation_WithGoodBatteryAndAvailableStatus_ShouldReturnTrue()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        // Act
        var isAvailable = vehicle.IsAvailableForReservation();

        // Assert
        Assert.True(isAvailable);
    }

    [Fact]
    public void IsAvailableForReservation_WithLowBattery_ShouldReturnFalse()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(15).Value; // Below 20% threshold
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        // Act
        var isAvailable = vehicle.IsAvailableForReservation();

        // Assert
        Assert.False(isAvailable);
    }

    [Fact]
    public void Reserve_WhenAvailable_ShouldSucceed()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        // Act
        var result = vehicle.Reserve();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Reserved, vehicle.Status);
    }

    [Fact]
    public void Reserve_WhenAlreadyReserved_ShouldFail()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;
        vehicle.Reserve();

        // Act
        var result = vehicle.Reserve();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.NotAvailable", result.Error.Code);
    }

    [Fact]
    public void Reserve_WithLowBattery_ShouldFail()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(15).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        // Act
        var result = vehicle.Reserve();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.NotAvailable", result.Error.Code);
    }

    [Fact]
    public void StartTrip_WhenReserved_ShouldSucceed()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;
        vehicle.Reserve();

        // Act
        var result = vehicle.StartTrip();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.InUse, vehicle.Status);
    }

    [Fact]
    public void StartTrip_WhenNotReserved_ShouldFail()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        // Act
        var result = vehicle.StartTrip();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.NotReserved", result.Error.Code);
    }

    [Fact]
    public void EndTrip_WhenInUse_ShouldSucceedAndUpdateLocation()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var startLocation = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, startLocation).Value;
        vehicle.Reserve();
        vehicle.StartTrip();

        var endLocation = Location.Create(34.0209, -6.8416).Value; // Different location

        // Act
        var result = vehicle.EndTrip(endLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.Equal(endLocation, vehicle.Location);
    }

    [Fact]
    public void EndTrip_WhenNotInUse_ShouldFail()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        var endLocation = Location.Create(34.0209, -6.8416).Value;

        // Act
        var result = vehicle.EndTrip(endLocation);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.NotInUse", result.Error.Code);
    }

    [Fact]
    public void EndTrip_WithLowBattery_ShouldSetStatusToUnavailable()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var startLocation = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, startLocation).Value;
        vehicle.Reserve();
        vehicle.StartTrip();

        // Update battery to low level
        var lowBattery = BatteryLevel.Create(10).Value;
        vehicle.UpdateBatteryLevel(lowBattery);

        var endLocation = Location.Create(34.0209, -6.8416).Value;

        // Act
        var result = vehicle.EndTrip(endLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Unavailable, vehicle.Status);
    }

    [Fact]
    public void UpdateLocation_WithValidLocation_ShouldSucceed()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        var newLocation = Location.Create(34.0209, -6.8416).Value;

        // Act
        var result = vehicle.UpdateLocation(newLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newLocation, vehicle.Location);
    }

    [Fact]
    public void UpdateBatteryLevel_WithValidLevel_ShouldSucceed()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        var newBatteryLevel = BatteryLevel.Create(50).Value;

        // Act
        var result = vehicle.UpdateBatteryLevel(newBatteryLevel);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newBatteryLevel, vehicle.BatteryLevel);
    }

    [Fact]
    public void UpdateBatteryLevel_ToLowLevel_ShouldSetStatusToUnavailable()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        var lowBatteryLevel = BatteryLevel.Create(10).Value;

        // Act
        var result = vehicle.UpdateBatteryLevel(lowBatteryLevel);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Unavailable, vehicle.Status);
    }

    [Fact]
    public void MarkForMaintenance_WhenNotInUse_ShouldSucceed()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        // Act
        var result = vehicle.MarkForMaintenance();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Maintenance, vehicle.Status);
    }

    [Fact]
    public void MarkForMaintenance_WhenInUse_ShouldFail()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;
        vehicle.Reserve();
        vehicle.StartTrip();

        // Act
        var result = vehicle.MarkForMaintenance();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.InUse", result.Error.Code);
    }

    [Fact]
    public void CompleteMaintenance_WhenInMaintenance_ShouldSucceed()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;
        vehicle.MarkForMaintenance();

        // Act
        var result = vehicle.CompleteMaintenance();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
    }

    [Fact]
    public void CompleteMaintenance_WhenNotInMaintenance_ShouldFail()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;

        // Act
        var result = vehicle.CompleteMaintenance();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.NotInMaintenance", result.Error.Code);
    }

    [Fact]
    public void CompleteMaintenance_WithLowBattery_ShouldSetStatusToUnavailable()
    {
        // Arrange
        var batteryLevel = BatteryLevel.Create(80).Value;
        var location = Location.Create(33.9716, -6.8498).Value;
        var vehicle = Vehicle.Create("BIKE-001", VehicleType.Bike, batteryLevel, location).Value;
        vehicle.MarkForMaintenance();

        // Update battery to low level
        var lowBattery = BatteryLevel.Create(10).Value;
        vehicle.UpdateBatteryLevel(lowBattery);

        // Act
        var result = vehicle.CompleteMaintenance();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Unavailable, vehicle.Status);
    }
}
