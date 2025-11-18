using EcoRide.BuildingBlocks.Application.Messaging;

namespace EcoRide.Modules.Security.Application.Commands.ResendOtp;

/// <summary>
/// Command to resend OTP code
/// </summary>
public sealed record ResendOtpCommand(
    string PhoneNumber
) : ICommand<string>;
