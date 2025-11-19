using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Trip.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ActiveTrip aggregate
/// </summary>
public sealed class ActiveTripRepository : IActiveTripRepository
{
    private readonly TripDbContext _context;

    public ActiveTripRepository(TripDbContext context)
    {
        _context = context;
    }

    public async Task<ActiveTrip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<ActiveTrip?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Where(t => t.UserId == userId && t.Status == TripStatus.Active)
            .OrderByDescending(t => t.StartTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ActiveTrip?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Where(t => t.VehicleId == vehicleId && t.Status == TripStatus.Active)
            .OrderByDescending(t => t.StartTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ActiveTrip>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ActiveTrip> Trips, int TotalCount)> GetTripHistoryAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Trips
            .Where(t => t.UserId == userId && t.Status == TripStatus.Completed)
            .OrderByDescending(t => t.StartTime);

        var totalCount = await query.CountAsync(cancellationToken);

        var trips = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (trips, totalCount);
    }

    public async Task AddAsync(ActiveTrip trip, CancellationToken cancellationToken = default)
    {
        await _context.Trips.AddAsync(trip, cancellationToken);
    }

    public void Update(ActiveTrip trip)
    {
        _context.Trips.Update(trip);
    }
}
