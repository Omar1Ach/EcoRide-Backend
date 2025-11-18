using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using ValueObjects = EcoRide.Modules.Fleet.Domain.ValueObjects;

namespace EcoRide.Modules.Fleet.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Vehicle aggregate with PostGIS spatial queries
/// </summary>
public sealed class VehicleRepository : IVehicleRepository
{
    private readonly FleetDbContext _context;

    public VehicleRepository(FleetDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vehicle?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Code == code, cancellationToken);
    }

    public async Task<List<Vehicle>> GetNearbyVehiclesAsync(
        ValueObjects.Location userLocation,
        int radiusMeters,
        CancellationToken cancellationToken = default)
    {
        // Use raw SQL with PostGIS ST_DWithin for spatial query
        // ST_DWithin works with geography type and uses meters for distance
        var vehicles = await _context.Vehicles
            .FromSqlRaw(@"
                SELECT * FROM fleet.vehicles
                WHERE status = {0}
                AND battery_level >= {1}
                AND ST_DWithin(
                    location,
                    ST_SetSRID(ST_MakePoint({2}, {3}), 4326)::geography,
                    {4}
                )",
                VehicleStatus.Available.ToString(),
                ValueObjects.BatteryLevel.LowBatteryThreshold,
                userLocation.Longitude,
                userLocation.Latitude,
                radiusMeters)
            .ToListAsync(cancellationToken);

        return vehicles;
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        await _context.Vehicles.AddAsync(vehicle, cancellationToken);
    }

    public void Update(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
    }
}
