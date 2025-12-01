namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Request DTO for updating user profile
/// </summary>
public sealed record UpdateProfileRequest(
    string FullName,
    string Email,
    string PhoneNumber
);
