namespace EcoRide.Api.Models.Auth;

/// <summary>
/// Request model for login
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password,
    bool Enable2FA = false  // Set to true to require 2FA for this login
);
