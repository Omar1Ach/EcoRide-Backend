using EcoRide.Modules.Security.Domain.Entities;
using EcoRide.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for wallet transactions
/// US-008: Wallet Management
/// </summary>
internal sealed class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly SecurityDbContext _context;

    public WalletTransactionRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .FirstOrDefaultAsync(wt => wt.Id == id, cancellationToken);
    }

    public async Task<(List<WalletTransaction> Transactions, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WalletTransactions
            .Where(wt => wt.UserId == userId)
            .OrderByDescending(wt => wt.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }

    public void Add(WalletTransaction transaction)
    {
        _context.WalletTransactions.Add(transaction);
    }
}
