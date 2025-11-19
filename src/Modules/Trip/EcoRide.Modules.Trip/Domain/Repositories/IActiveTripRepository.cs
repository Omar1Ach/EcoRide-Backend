using EcoRide.Modules.Trip.Domain.Aggregates;

namespace EcoRide.Modules.Trip.Domain.Repositories;

/// <summary>
/// Repository interface for ActiveTrip aggregate
/// </summary>
public interface IActiveTripRepository
{
    Task<ActiveTrip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActiveTrip?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ActiveTrip?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    Task<List<ActiveTrip>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(ActiveTrip trip, CancellationToken cancellationToken = default);

    void Update(ActiveTrip trip);
}
