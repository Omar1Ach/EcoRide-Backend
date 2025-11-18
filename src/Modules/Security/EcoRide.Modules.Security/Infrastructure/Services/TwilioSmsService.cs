using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EcoRide.Modules.Security.Infrastructure.Services;

/// <summary>
/// Twilio-based SMS service implementation
/// </summary>
public sealed class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var accountSid = _configuration["Twilio:AccountSid"];
        var authToken = _configuration["Twilio:AuthToken"];

        if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
        {
            TwilioClient.Init(accountSid, authToken);
        }
    }

    public async Task<Result> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromPhoneNumber = _configuration["Twilio:PhoneNumber"];

            if (string.IsNullOrEmpty(fromPhoneNumber))
            {
                _logger.LogError("Twilio phone number not configured");
                return Result.Failure(new Error("Sms.ConfigurationError", "SMS service is not properly configured"));
            }

            var message = $"Your EcoRide verification code is: {code}. Valid for 5 minutes.";

            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(fromPhoneNumber),
                body: message);

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {MessageSid}", phoneNumber, messageResource.Sid);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return Result.Failure(new Error("Sms.SendFailed", "Failed to send SMS. Please try again later."));
        }
    }
}
