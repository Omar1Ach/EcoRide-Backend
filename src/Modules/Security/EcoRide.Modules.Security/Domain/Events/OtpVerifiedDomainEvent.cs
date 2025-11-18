using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.Events;

/// <summary>
/// Raised when OTP is successfully verified
/// </summary>
public sealed record OtpVerifiedDomainEvent(
    Guid UserId,
    string PhoneNumber,
    DateTime VerifiedAt
) : DomainEvent;
