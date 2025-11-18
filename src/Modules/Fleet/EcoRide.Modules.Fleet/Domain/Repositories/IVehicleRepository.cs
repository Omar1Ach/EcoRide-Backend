using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.ValueObjects;

namespace EcoRide.Modules.Fleet.Domain.Repositories;

/// <summary>
/// Repository interface for Vehicle aggregate
/// </summary>
public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<Vehicle>> GetNearbyVehiclesAsync(Location userLocation, int radiusMeters, CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    void Update(Vehicle vehicle);
}
