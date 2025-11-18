namespace EcoRide.BuildingBlocks.Application.Data;

/// <summary>
/// Unit of Work pattern for managing database transactions
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the current transaction
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
