using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.ValueObjects;

namespace EcoRide.Modules.Fleet.Domain.Aggregates;

/// <summary>
/// Vehicle aggregate root for bikes and scooters
/// </summary>
public sealed class Vehicle : AggregateRoot<Guid>
{
    public string Code { get; private set; } = null!;
    public VehicleType Type { get; private set; }
    public VehicleStatus Status { get; private set; }
    public BatteryLevel BatteryLevel { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public DateTime LastLocationUpdate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private Vehicle() { }

    private Vehicle(
        Guid id,
        string code,
        VehicleType type,
        BatteryLevel batteryLevel,
        Location location)
    {
        Id = id;
        Code = code;
        Type = type;
        BatteryLevel = batteryLevel;
        Location = location;
        Status = VehicleStatus.Available;
        LastLocationUpdate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new vehicle
    /// </summary>
    public static Result<Vehicle> Create(
        string code,
        VehicleType type,
        BatteryLevel batteryLevel,
        Location location)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Vehicle>(
                new Error("Vehicle.CodeEmpty", "Vehicle code cannot be empty"));
        }

        var vehicle = new Vehicle(
            Guid.NewGuid(),
            code,
            type,
            batteryLevel,
            location);

        return Result.Success(vehicle);
    }

    /// <summary>
    /// Checks if vehicle is available for reservation
    /// </summary>
    public bool IsAvailableForReservation()
    {
        return Status == VehicleStatus.Available && BatteryLevel.IsAvailable();
    }

    /// <summary>
    /// Updates vehicle location
    /// </summary>
    public Result UpdateLocation(Location newLocation)
    {
        Location = newLocation;
        LastLocationUpdate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Updates battery level
    /// </summary>
    public Result UpdateBatteryLevel(BatteryLevel newLevel)
    {
        BatteryLevel = newLevel;
        UpdatedAt = DateTime.UtcNow;

        // Auto-mark as unavailable if battery too low
        if (BatteryLevel.IsLow() && Status == VehicleStatus.Available)
        {
            Status = VehicleStatus.Unavailable;
        }

        return Result.Success();
    }

    /// <summary>
    /// Reserves the vehicle
    /// </summary>
    public Result Reserve()
    {
        if (!IsAvailableForReservation())
        {
            return Result.Failure(
                new Error("Vehicle.NotAvailable", "Vehicle is not available for reservation"));
        }

        Status = VehicleStatus.Reserved;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Starts a trip with this vehicle
    /// </summary>
    public Result StartTrip()
    {
        if (Status != VehicleStatus.Reserved)
        {
            return Result.Failure(
                new Error("Vehicle.NotReserved", "Vehicle must be reserved before starting a trip"));
        }

        Status = VehicleStatus.InUse;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Ends a trip and makes vehicle available again
    /// </summary>
    public Result EndTrip(Location endLocation)
    {
        if (Status != VehicleStatus.InUse)
        {
            return Result.Failure(
                new Error("Vehicle.NotInUse", "Vehicle is not currently in use"));
        }

        Location = endLocation;
        Status = BatteryLevel.IsAvailable() ? VehicleStatus.Available : VehicleStatus.Unavailable;
        LastLocationUpdate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Marks vehicle for maintenance
    /// </summary>
    public Result MarkForMaintenance()
    {
        if (Status == VehicleStatus.InUse)
        {
            return Result.Failure(
                new Error("Vehicle.InUse", "Cannot mark vehicle for maintenance while in use"));
        }

        Status = VehicleStatus.Maintenance;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Completes maintenance and makes vehicle available
    /// </summary>
    public Result CompleteMaintenance()
    {
        if (Status != VehicleStatus.Maintenance)
        {
            return Result.Failure(
                new Error("Vehicle.NotInMaintenance", "Vehicle is not in maintenance"));
        }

        Status = BatteryLevel.IsAvailable() ? VehicleStatus.Available : VehicleStatus.Unavailable;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
