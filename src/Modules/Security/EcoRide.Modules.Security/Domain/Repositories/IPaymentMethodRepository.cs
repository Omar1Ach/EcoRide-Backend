using EcoRide.Modules.Security.Domain.Entities;

namespace EcoRide.Modules.Security.Domain.Repositories;

/// <summary>
/// Repository for managing user payment methods
/// US-006: Credit card payment fallback
/// </summary>
public interface IPaymentMethodRepository
{
    Task<PaymentMethodEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PaymentMethodEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentMethodEntity?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(PaymentMethodEntity paymentMethod);
    void Remove(PaymentMethodEntity paymentMethod);
}
