using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

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
        Location userLocation,
        int radiusMeters,
        CancellationToken cancellationToken = default)
    {
        // Create PostGIS Point for user location
        var userPoint = new Point(userLocation.Longitude, userLocation.Latitude) { SRID = 4326 };

        // Query using PostGIS ST_DWithin function
        // ST_DWithin works with geography type and uses meters for distance
        var vehicles = await _context.Vehicles
            .Where(v =>
                // Filter by available status and good battery level
                (v.Status == VehicleStatus.Available &&
                 v.BatteryLevel.Value >= BatteryLevel.LowBatteryThreshold) &&
                // Spatial filter: within radius
                EF.Functions.IsWithinDistance(v.Location, userPoint, radiusMeters))
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
