using EcoRide.Modules.Trip.Domain.Aggregates;

namespace EcoRide.Modules.Trip.Domain.Repositories;

/// <summary>
/// Repository for Reservation aggregate persistence
/// </summary>
public interface IReservationRepository
{
    /// <summary>
    /// Gets a reservation by ID
    /// </summary>
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active reservation for a specific user
    /// BR-002: User can reserve only 1 vehicle at a time
    /// </summary>
    Task<Reservation?> GetActiveReservationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active reservation for a specific vehicle
    /// Prevents multiple users from reserving the same vehicle
    /// </summary>
    Task<Reservation?> GetActiveReservationByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active reservations that have expired (for background cleanup)
    /// </summary>
    Task<List<Reservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new reservation
    /// </summary>
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing reservation
    /// </summary>
    void Update(Reservation reservation);
}
