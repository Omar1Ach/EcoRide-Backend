using EcoRide.Modules.Trip.Domain.Entities;
using EcoRide.Modules.Trip.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Trip.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Receipt entity
/// US-006: End Trip & Payment - Receipt management
/// </summary>
public sealed class ReceiptRepository : IReceiptRepository
{
    private readonly TripDbContext _context;

    public ReceiptRepository(TripDbContext context)
    {
        _context = context;
    }

    public async Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Receipt?> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Receipts
            .FirstOrDefaultAsync(r => r.TripId == tripId, cancellationToken);
    }

    public async Task<List<Receipt>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Receipts
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public void Add(Receipt receipt)
    {
        _context.Receipts.Add(receipt);
    }
}
