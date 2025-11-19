using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.Modules.Trip.Domain.Aggregates;

/// <summary>
/// Trip aggregate - represents an active rental trip
/// Business Rules (BR-004):
/// - Base cost: 5 MAD
/// - Per-minute rate: 1.5 MAD/min
/// - Cost rounds to nearest MAD
/// - Mock distance: +100m/minute
/// - Trip must start from active reservation
/// </summary>
public sealed class ActiveTrip : AggregateRoot<Guid>
{
    public const decimal BaseCostMAD = 5.0m;
    public const decimal PerMinuteRateMAD = 1.5m;
    public const int MockDistanceMetersPerMinute = 100;

    public Guid UserId { get; private set; }
    public Guid VehicleId { get; private set; }
    public Guid? ReservationId { get; private set; }
    public TripStatus Status { get; private set; }

    public DateTime StartTime { get; private set; }
    public double StartLatitude { get; private set; }
    public double StartLongitude { get; private set; }

    public DateTime? EndTime { get; private set; }
    public double? EndLatitude { get; private set; }
    public double? EndLongitude { get; private set; }

    public decimal TotalCost { get; private set; }
    public int DurationMinutes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private ActiveTrip() { }

    private ActiveTrip(
        Guid id,
        Guid userId,
        Guid vehicleId,
        Guid? reservationId,
        Location startLocation,
        DateTime startTime)
    {
        Id = id;
        UserId = userId;
        VehicleId = vehicleId;
        ReservationId = reservationId;
        Status = TripStatus.Active;

        StartTime = startTime;
        StartLatitude = startLocation.Latitude;
        StartLongitude = startLocation.Longitude;

        TotalCost = BaseCostMAD; // BR-003: Base cost
        DurationMinutes = 0;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Start a new trip from reservation
    /// </summary>
    public static Result<ActiveTrip> Start(
        Guid userId,
        Guid vehicleId,
        Guid? reservationId,
        Location startLocation)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<ActiveTrip>(new Error(
                "Trip.InvalidUserId",
                "User ID is required"));
        }

        if (vehicleId == Guid.Empty)
        {
            return Result.Failure<ActiveTrip>(new Error(
                "Trip.InvalidVehicleId",
                "Vehicle ID is required"));
        }

        var trip = new ActiveTrip(
            Guid.NewGuid(),
            userId,
            vehicleId,
            reservationId,
            startLocation,
            DateTime.UtcNow);

        return Result.Success(trip);
    }

    /// <summary>
    /// End the trip and calculate final cost
    /// </summary>
    public Result End(Location endLocation)
    {
        if (Status != TripStatus.Active)
        {
            return Result.Failure(new Error(
                "Trip.NotActive",
                "Cannot end trip that is not active"));
        }

        EndTime = DateTime.UtcNow;
        EndLatitude = endLocation.Latitude;
        EndLongitude = endLocation.Longitude;
        Status = TripStatus.Completed;

        // Calculate duration in minutes
        var duration = EndTime.Value - StartTime;
        DurationMinutes = (int)Math.Ceiling(duration.TotalMinutes);

        // Calculate total cost: Base + (Minutes * Rate), rounded to nearest MAD (BR-004)
        var cost = BaseCostMAD + (DurationMinutes * PerMinuteRateMAD);
        TotalCost = Math.Round(cost, 0, MidpointRounding.AwayFromZero);

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Cancel the active trip
    /// </summary>
    public Result Cancel()
    {
        if (Status != TripStatus.Active)
        {
            return Result.Failure(new Error(
                "Trip.NotActive",
                "Cannot cancel trip that is not active"));
        }

        Status = TripStatus.Cancelled;
        EndTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Get current trip duration in minutes
    /// </summary>
    public int GetCurrentDurationMinutes()
    {
        if (Status != TripStatus.Active)
        {
            return DurationMinutes;
        }

        var duration = DateTime.UtcNow - StartTime;
        return (int)Math.Ceiling(duration.TotalMinutes);
    }

    /// <summary>
    /// Get current estimated cost (BR-004: rounded to nearest MAD)
    /// </summary>
    public decimal GetCurrentEstimatedCost()
    {
        if (Status != TripStatus.Active)
        {
            return TotalCost;
        }

        var currentMinutes = GetCurrentDurationMinutes();
        var cost = BaseCostMAD + (currentMinutes * PerMinuteRateMAD);
        return Math.Round(cost, 0, MidpointRounding.AwayFromZero); // Round to nearest MAD
    }

    /// <summary>
    /// Get mock distance traveled (BR-004: +100m/minute)
    /// </summary>
    public int GetMockDistanceMeters()
    {
        if (Status != TripStatus.Active)
        {
            return DurationMinutes * MockDistanceMetersPerMinute;
        }

        var currentMinutes = GetCurrentDurationMinutes();
        return currentMinutes * MockDistanceMetersPerMinute;
    }

    public bool IsActive() => Status == TripStatus.Active;
}
