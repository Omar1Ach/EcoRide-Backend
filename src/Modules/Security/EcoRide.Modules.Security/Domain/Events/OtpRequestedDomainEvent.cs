using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.Events;

/// <summary>
/// Raised when OTP is generated and sent
/// </summary>
public sealed record OtpRequestedDomainEvent(
    string PhoneNumber,
    DateTime RequestedAt,
    DateTime ExpiresAt
) : DomainEvent;
