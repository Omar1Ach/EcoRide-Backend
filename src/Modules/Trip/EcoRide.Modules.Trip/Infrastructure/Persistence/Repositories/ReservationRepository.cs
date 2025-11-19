using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Trip.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Reservation aggregate
/// </summary>
public sealed class ReservationRepository : IReservationRepository
{
    private readonly TripDbContext _context;

    public ReservationRepository(TripDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Reservation?> GetActiveReservationByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => r.UserId == userId && r.Status == ReservationStatus.Active)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Reservation?> GetActiveReservationByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => r.VehicleId == vehicleId && r.Status == ReservationStatus.Active)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Reservation>> GetExpiredReservationsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.Reservations
            .Where(r =>
                r.Status == ReservationStatus.Active &&
                r.ExpiresAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.AddAsync(reservation, cancellationToken);
    }

    public void Update(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
    }
}
