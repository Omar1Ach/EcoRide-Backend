using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Domain.Events;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Domain.Aggregates;

/// <summary>
/// OTP code aggregate root managing OTP lifecycle and verification
/// </summary>
public sealed class OtpCode : AggregateRoot<long>
{
    public const int MaxAttempts = 3;
    public const int ValidityMinutes = 5;

    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public int Attempts { get; private set; }
    public bool Verified { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private constructor for EF Core
    private OtpCode() { }

    private OtpCode(PhoneNumber phoneNumber, string code, DateTime expiresAt)
    {
        PhoneNumber = phoneNumber;
        Code = code;
        ExpiresAt = expiresAt;
        Attempts = 0;
        Verified = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to generate a new OTP code
    /// </summary>
    public static OtpCode Generate(PhoneNumber phoneNumber)
    {
        var code = GenerateRandomCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(ValidityMinutes);

        var otp = new OtpCode(phoneNumber, code, expiresAt);

        otp.AddDomainEvent(new OtpRequestedDomainEvent(
            phoneNumber,
            otp.CreatedAt,
            expiresAt));

        return otp;
    }

    /// <summary>
    /// Verifies the OTP code
    /// </summary>
    public Result<bool> Verify(string inputCode)
    {
        if (IsExpired())
        {
            return Result.Failure<bool>(
                new Error("Otp.Expired", "OTP code has expired"));
        }

        if (Attempts >= MaxAttempts)
        {
            return Result.Failure<bool>(
                new Error("Otp.MaxAttemptsExceeded", "Maximum OTP verification attempts exceeded"));
        }

        Attempts++;

        if (Code != inputCode)
        {
            return Result.Failure<bool>(
                new Error("Otp.InvalidCode", "Invalid OTP code"));
        }

        Verified = true;
        return Result.Success(true);
    }

    /// <summary>
    /// Checks if the OTP has expired
    /// </summary>
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if retry is allowed
    /// </summary>
    public bool CanRetry() => Attempts < MaxAttempts;

    /// <summary>
    /// Generates a random 6-digit OTP code
    /// </summary>
    private static string GenerateRandomCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
