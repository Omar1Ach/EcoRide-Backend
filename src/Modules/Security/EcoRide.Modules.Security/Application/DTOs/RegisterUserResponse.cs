namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Response returned after successful user registration
/// </summary>
public sealed record RegisterUserResponse(
    Guid UserId,
    string Message
);
