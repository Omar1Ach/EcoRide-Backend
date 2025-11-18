namespace EcoRide.Modules.Security.Application.Services;

/// <summary>
/// Service for generating JWT tokens
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT access token (15 minutes expiry)
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, string role);

    /// <summary>
    /// Generates a refresh token (7 days expiry)
    /// </summary>
    string GenerateRefreshToken();
}
