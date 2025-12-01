using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Domain.Entities;
using EcoRide.Modules.Security.Domain.Repositories;

namespace EcoRide.Modules.Security.Application.Queries.GetUserSettings;

public sealed class GetUserSettingsQueryHandler : IQueryHandler<GetUserSettingsQuery, UserSettingsDto>
{
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly IUserRepository _userRepository;

    public GetUserSettingsQueryHandler(
        IUserSettingsRepository settingsRepository,
        IUserRepository userRepository)
    {
        _settingsRepository = settingsRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<UserSettingsDto>> Handle(
        GetUserSettingsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserSettingsDto>(
                new Error("User.NotFound", "User not found"));
        }

        // Get or create settings
        var settings = await _settingsRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (settings is null)
        {
            // Return default settings if not found
            return Result.Success(new UserSettingsDto(
                PushNotificationsEnabled: true,
                DarkModeEnabled: false,
                HapticFeedbackEnabled: true,
                LanguageCode: "en",
                UpdatedAt: null));
        }

        var settingsDto = new UserSettingsDto(
            settings.PushNotificationsEnabled,
            settings.DarkModeEnabled,
            settings.HapticFeedbackEnabled,
            settings.LanguageCode,
            settings.UpdatedAt);

        return Result.Success(settingsDto);
    }
}
