using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Commands.RefreshToken;

/// <summary>
/// Command to refresh access token using refresh token
/// </summary>
public sealed record RefreshTokenCommand(
    Guid UserId,
    string RefreshToken
) : ICommand<RefreshTokenResponse>;
