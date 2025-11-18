namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Response returned after successful OTP verification
/// </summary>
public sealed record VerifyOtpResponse(
    Guid UserId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
