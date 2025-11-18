using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Commands.VerifyOtp;

/// <summary>
/// Command to verify OTP code
/// </summary>
public sealed record VerifyOtpCommand(
    string PhoneNumber,
    string Code
) : ICommand<VerifyOtpResponse>;
