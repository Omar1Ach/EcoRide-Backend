using EcoRide.BuildingBlocks.Application.Messaging;

namespace EcoRide.Modules.Security.Application.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process
/// </summary>
public sealed record ForgotPasswordCommand(
    string Email
) : ICommand<string>;
