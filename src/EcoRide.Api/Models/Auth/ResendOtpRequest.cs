namespace EcoRide.Api.Models.Auth;

/// <summary>
/// Request model for resending OTP
/// </summary>
public sealed record ResendOtpRequest(
    string PhoneNumber
);
