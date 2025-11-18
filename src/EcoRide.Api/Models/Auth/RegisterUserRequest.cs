namespace EcoRide.Api.Models.Auth;

/// <summary>
/// Request model for user registration
/// </summary>
public sealed record RegisterUserRequest(
    string Email,
    string PhoneNumber,
    string Password,
    string FullName
);
