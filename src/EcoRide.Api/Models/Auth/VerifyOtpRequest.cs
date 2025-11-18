namespace EcoRide.Api.Models.Auth;

/// <summary>
/// Request model for OTP verification
/// </summary>
public sealed record VerifyOtpRequest(
    string PhoneNumber,
    string Code
);
