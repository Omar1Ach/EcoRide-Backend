using EcoRide.Modules.Trip.Domain.Entities;

namespace EcoRide.Modules.Trip.Domain.Repositories;

/// <summary>
/// Repository for managing trip receipts
/// US-006: End Trip & Payment - Receipt management
/// </summary>
public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Receipt?> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task<List<Receipt>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    void Add(Receipt receipt);
}
