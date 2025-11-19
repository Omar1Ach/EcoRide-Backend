namespace EcoRide.Modules.Security.Application.Data;

/// <summary>
/// Unit of Work specific to the Security module
/// Provides transaction management for security-related operations
/// </summary>
public interface ISecurityUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the current transaction to the security database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
