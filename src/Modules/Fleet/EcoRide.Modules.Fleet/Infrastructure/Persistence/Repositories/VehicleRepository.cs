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

    public async Task<(List<Vehicle> Vehicles, int TotalCount)> GetAllAsync(
        string? status,
        string? type,
        int? minBatteryLevel,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vehicles.AsQueryable();

        // Apply filters - parse strings to enums to avoid ToString() in LINQ
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VehicleStatus>(status, out var statusEnum))
        {
            query = query.Where(v => v.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<VehicleType>(type, out var typeEnum))
        {
            query = query.Where(v => v.Type == typeEnum);
        }

        // For battery level with value objects, use client evaluation to avoid EF Core translation issues
        List<Vehicle> allVehicles;
        int totalCount;

        if (minBatteryLevel.HasValue)
        {
            // Fetch from database first (with status/type filters applied)
            var tempResults = await query.OrderByDescending(v => v.CreatedAt).ToListAsync(cancellationToken);

            // Apply battery filter in memory
            var filtered = tempResults.Where(v => v.BatteryLevel.Value >= minBatteryLevel.Value).ToList();
            totalCount = filtered.Count;

            // Apply pagination in memory
            allVehicles = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        else
        {
            // No battery filter - use normal database pagination
            totalCount = await query.CountAsync(cancellationToken);

            allVehicles = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        return (allVehicles, totalCount);
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
