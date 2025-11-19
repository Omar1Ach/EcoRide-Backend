using EcoRide.BuildingBlocks.Application.Messaging;

namespace EcoRide.Modules.Security.Application.Commands.ResetPassword;

/// <summary>
/// Command to reset password using reset code
/// </summary>
public sealed record ResetPasswordCommand(
    string Email,
    string ResetCode,
    string NewPassword
) : ICommand<string>;
