using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Commands.UpdateUserSettings;

/// <summary>
/// Command to update user settings
/// </summary>
public sealed record UpdateUserSettingsCommand(
    Guid UserId,
    bool PushNotificationsEnabled,
    bool DarkModeEnabled,
    bool HapticFeedbackEnabled,
    string LanguageCode
) : ICommand<UserSettingsDto>;
