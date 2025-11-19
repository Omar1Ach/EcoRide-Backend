namespace EcoRide.Api.Models.Auth;

/// <summary>
/// Request model for refresh token
/// </summary>
public sealed record RefreshTokenRequest(
    Guid UserId,
    string RefreshToken
);
