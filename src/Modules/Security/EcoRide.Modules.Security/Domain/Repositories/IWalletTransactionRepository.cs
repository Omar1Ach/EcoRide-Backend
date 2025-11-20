using EcoRide.Modules.Security.Domain.Entities;

namespace EcoRide.Modules.Security.Domain.Repositories;

/// <summary>
/// Repository for wallet transactions
/// US-008: Wallet Management
/// </summary>
public interface IWalletTransactionRepository
{
    Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<WalletTransaction> Transactions, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    void Add(WalletTransaction transaction);
}
