using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.Events;

/// <summary>
/// Raised when a new user completes registration
/// </summary>
public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string PhoneNumber,
    string FullName,
    DateTime RegisteredAt
) : DomainEvent;
