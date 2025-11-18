namespace EcoRide.BuildingBlocks.Domain;

/// <summary>
/// Base class for aggregate roots.
/// Aggregate roots are the entry points to aggregates and are responsible for maintaining invariants.
/// They also manage domain events that occur within the aggregate.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events raised by this aggregate
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Adds a domain event to be published
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events (called after publishing)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
