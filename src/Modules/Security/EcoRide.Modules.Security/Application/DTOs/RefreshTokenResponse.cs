namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Response DTO for refresh token operation
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,  // New refresh token (token rotation)
    DateTime ExpiresAt
);
