using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Domain.Enums;

namespace EcoRide.Modules.Trip.Domain.Aggregates;

/// <summary>
/// Reservation aggregate root - manages 5-minute vehicle holds
/// Business Rules (BR-002):
/// - User can reserve only 1 vehicle at a time
/// - Reservation duration: 5 minutes (300 seconds)
/// - No penalty for manual cancellation
/// - Auto-expires if not converted to trip
/// </summary>
public sealed class Reservation : AggregateRoot<Guid>
{
    public const int ReservationDurationSeconds = 300; // 5 minutes

    public Guid UserId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? ConvertedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private Reservation() { }

    private Reservation(
        Guid id,
        Guid userId,
        Guid vehicleId,
        DateTime createdAt,
        DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        VehicleId = vehicleId;
        Status = ReservationStatus.Active;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Factory method to create a new reservation
    /// </summary>
    public static Result<Reservation> Create(Guid userId, Guid vehicleId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Reservation>(
                new Error("Reservation.InvalidUserId", "User ID cannot be empty"));
        }

        if (vehicleId == Guid.Empty)
        {
            return Result.Failure<Reservation>(
                new Error("Reservation.InvalidVehicleId", "Vehicle ID cannot be empty"));
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(ReservationDurationSeconds);

        var reservation = new Reservation(
            Guid.NewGuid(),
            userId,
            vehicleId,
            now,
            expiresAt);

        return Result.Success(reservation);
    }

    /// <summary>
    /// Checks if reservation is still active (not expired, cancelled, or converted)
    /// </summary>
    public bool IsActive()
    {
        return Status == ReservationStatus.Active && DateTime.UtcNow < ExpiresAt;
    }

    /// <summary>
    /// Checks if reservation has expired based on current time
    /// </summary>
    public bool HasExpired()
    {
        return Status == ReservationStatus.Active && DateTime.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// Gets remaining time in seconds (for countdown timer)
    /// </summary>
    public int GetRemainingSeconds()
    {
        if (Status != ReservationStatus.Active)
        {
            return 0;
        }

        var remaining = (ExpiresAt - DateTime.UtcNow).TotalSeconds;
        return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
    }

    /// <summary>
    /// Cancels the reservation manually (user action)
    /// BR-002: No penalty for manual cancellation
    /// </summary>
    public Result Cancel()
    {
        if (Status != ReservationStatus.Active)
        {
            return Result.Failure(
                new Error("Reservation.NotActive", "Cannot cancel a reservation that is not active"));
        }

        if (HasExpired())
        {
            return Result.Failure(
                new Error("Reservation.AlreadyExpired", "Cannot cancel an expired reservation"));
        }

        Status = ReservationStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Marks reservation as expired (automatic process)
    /// </summary>
    public Result MarkAsExpired()
    {
        if (Status != ReservationStatus.Active)
        {
            return Result.Failure(
                new Error("Reservation.NotActive", "Cannot expire a reservation that is not active"));
        }

        if (!HasExpired())
        {
            return Result.Failure(
                new Error("Reservation.NotYetExpired", "Cannot expire a reservation that hasn't reached expiry time"));
        }

        Status = ReservationStatus.Expired;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Converts reservation to trip (when QR code is scanned)
    /// </summary>
    public Result ConvertToTrip()
    {
        if (Status != ReservationStatus.Active)
        {
            return Result.Failure(
                new Error("Reservation.NotActive", "Cannot convert a reservation that is not active"));
        }

        if (HasExpired())
        {
            return Result.Failure(
                new Error("Reservation.Expired", "Cannot convert an expired reservation"));
        }

        Status = ReservationStatus.Converted;
        ConvertedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
