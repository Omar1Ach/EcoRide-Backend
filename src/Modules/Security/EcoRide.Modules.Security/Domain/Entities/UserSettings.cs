using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.Entities;

/// <summary>
/// User settings entity for managing user preferences
/// </summary>
public sealed class UserSettings : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public bool PushNotificationsEnabled { get; private set; }
    public bool DarkModeEnabled { get; private set; }
    public bool HapticFeedbackEnabled { get; private set; }
    public string LanguageCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private UserSettings()
    {
        LanguageCode = "en";
    }

    private UserSettings(
        Guid id,
        Guid userId,
        bool pushNotificationsEnabled,
        bool darkModeEnabled,
        bool hapticFeedbackEnabled,
        string languageCode)
    {
        Id = id;
        UserId = userId;
        PushNotificationsEnabled = pushNotificationsEnabled;
        DarkModeEnabled = darkModeEnabled;
        HapticFeedbackEnabled = hapticFeedbackEnabled;
        LanguageCode = languageCode;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Create default user settings
    /// </summary>
    public static Result<UserSettings> CreateDefault(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<UserSettings>(
                new Error("UserSettings.InvalidUserId", "User ID is required"));
        }

        var settings = new UserSettings(
            Guid.NewGuid(),
            userId,
            pushNotificationsEnabled: true,
            darkModeEnabled: false,
            hapticFeedbackEnabled: true,
            languageCode: "en");

        return Result.Success(settings);
    }

    /// <summary>
    /// Update user settings
    /// </summary>
    public Result Update(
        bool pushNotificationsEnabled,
        bool darkModeEnabled,
        bool hapticFeedbackEnabled,
        string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return Result.Failure(
                new Error("UserSettings.InvalidLanguageCode", "Language code is required"));
        }

        // Validate language code (en, fr, ar, es)
        var validLanguages = new[] { "en", "fr", "ar", "es" };
        if (!validLanguages.Contains(languageCode.ToLower()))
        {
            return Result.Failure(
                new Error("UserSettings.UnsupportedLanguage",
                    "Supported languages: en, fr, ar, es"));
        }

        PushNotificationsEnabled = pushNotificationsEnabled;
        DarkModeEnabled = darkModeEnabled;
        HapticFeedbackEnabled = hapticFeedbackEnabled;
        LanguageCode = languageCode.ToLower();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
