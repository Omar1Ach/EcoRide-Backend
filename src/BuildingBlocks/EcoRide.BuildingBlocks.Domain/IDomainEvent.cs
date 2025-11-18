namespace EcoRide.BuildingBlocks.Domain;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base record for domain events with default implementation
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
