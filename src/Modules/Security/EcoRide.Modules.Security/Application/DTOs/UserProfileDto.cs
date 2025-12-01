namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Response DTO for user profile information
/// </summary>
public sealed record UserProfileDto(
    Guid UserId,
    string FullName,
    string Email,
    string PhoneNumber,
    bool IsEmailVerified,
    bool IsPhoneVerified,
    string KycStatus,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
