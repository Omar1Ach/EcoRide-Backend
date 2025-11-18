using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Application.Services;

/// <summary>
/// Service for sending SMS messages via Twilio
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an OTP code via SMS
    /// </summary>
    Task<Result> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
