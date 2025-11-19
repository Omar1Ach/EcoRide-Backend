namespace EcoRide.Api.Models.Auth;

/// <summary>
/// Request model for reset password
/// </summary>
public sealed record ResetPasswordRequest(
    string Email,
    string ResetCode,  // 6-digit code sent via SMS
    string NewPassword
);
