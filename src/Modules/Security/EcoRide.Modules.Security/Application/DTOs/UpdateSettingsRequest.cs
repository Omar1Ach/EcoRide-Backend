namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Request DTO for updating user settings
/// </summary>
public sealed record UpdateSettingsRequest(
    bool PushNotificationsEnabled,
    bool DarkModeEnabled,
    bool HapticFeedbackEnabled,
    string LanguageCode
);
