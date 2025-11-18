namespace EcoRide.BuildingBlocks.Domain;

/// <summary>
/// Generic repository interface for aggregate roots.
/// Provides basic CRUD operations following the Repository pattern.
/// </summary>
/// <typeparam name="TEntity">The aggregate root type</typeparam>
/// <typeparam name="TId">The identifier type</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity from the repository
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
