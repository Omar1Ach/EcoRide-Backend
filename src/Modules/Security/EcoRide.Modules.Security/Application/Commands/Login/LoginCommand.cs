using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Commands.Login;

/// <summary>
/// Command to authenticate a user with email and password
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    bool Enable2FA = false  // Optional: send OTP for 2FA if enabled on account
) : ICommand<LoginResponse>;
