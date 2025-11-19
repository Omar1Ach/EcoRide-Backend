namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Response DTO for login operation
/// </summary>
public sealed record LoginResponse(
    Guid UserId,
    string Email,
    string? AccessToken,      // Null if 2FA required
    string? RefreshToken,     // Null if 2FA required
    DateTime? ExpiresAt,      // Null if 2FA required
    bool Requires2FA,         // True if user must verify OTP
    string Message
);
