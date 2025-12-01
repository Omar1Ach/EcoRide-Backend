namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// Response DTO for user settings
/// </summary>
public sealed record UserSettingsDto(
    bool PushNotificationsEnabled,
    bool DarkModeEnabled,
    bool HapticFeedbackEnabled,
    string LanguageCode,
    DateTime? UpdatedAt
);
