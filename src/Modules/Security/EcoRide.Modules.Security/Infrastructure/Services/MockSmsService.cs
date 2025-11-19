using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Services;
using Microsoft.Extensions.Logging;

namespace EcoRide.Modules.Security.Infrastructure.Services;

/// <summary>
/// Mock SMS service for development/testing
/// Logs OTP codes to console instead of sending real SMS
/// </summary>
public sealed class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("🔐 MOCK SMS SERVICE - OTP Code for {PhoneNumber}: {Code} (Valid for 5 minutes)", phoneNumber, code);
        Console.WriteLine($"\n📱 ========================================");
        Console.WriteLine($"📱 MOCK SMS TO: {phoneNumber}");
        Console.WriteLine($"📱 OTP CODE: {code}");
        Console.WriteLine($"📱 (Use this code to verify registration)");
        Console.WriteLine($"📱 ========================================\n");

        return Task.FromResult(Result.Success());
    }
}
