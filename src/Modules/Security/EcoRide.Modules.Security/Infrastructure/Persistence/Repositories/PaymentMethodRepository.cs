using EcoRide.Modules.Security.Domain.Entities;
using EcoRide.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for PaymentMethodEntity
/// US-006: Credit card payment fallback
/// </summary>
public sealed class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly SecurityDbContext _context;

    public PaymentMethodRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentMethodEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
    }

    public async Task<List<PaymentMethodEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethodEntity?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault, cancellationToken);
    }

    public void Add(PaymentMethodEntity paymentMethod)
    {
        _context.PaymentMethods.Add(paymentMethod);
    }

    public void Remove(PaymentMethodEntity paymentMethod)
    {
        _context.PaymentMethods.Remove(paymentMethod);
    }
}
