namespace EcoRide.Api.Models.Auth;

/// <summary>
/// Request model for forgot password
/// </summary>
public sealed record ForgotPasswordRequest(
    string Email
);
