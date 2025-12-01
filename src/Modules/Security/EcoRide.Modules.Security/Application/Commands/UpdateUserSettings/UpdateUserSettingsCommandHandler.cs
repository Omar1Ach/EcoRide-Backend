using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Domain.Entities;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Infrastructure.Persistence;

namespace EcoRide.Modules.Security.Application.Commands.UpdateUserSettings;

/// <summary>
/// Handler for UpdateUserSettingsCommand
/// </summary>
public sealed class UpdateUserSettingsCommandHandler : ICommandHandler<UpdateUserSettingsCommand, UserSettingsDto>
{
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly IUserRepository _userRepository;
    private readonly SecurityDbContext _dbContext;

    public UpdateUserSettingsCommandHandler(
        IUserSettingsRepository settingsRepository,
        IUserRepository userRepository,
        SecurityDbContext dbContext)
    {
        _settingsRepository = settingsRepository;
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<UserSettingsDto>> Handle(
        UpdateUserSettingsCommand request,
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
            // Create new settings
            var createResult = UserSettings.CreateDefault(request.UserId);
            if (createResult.IsFailure)
            {
                return Result.Failure<UserSettingsDto>(createResult.Error);
            }

            settings = createResult.Value;
            await _settingsRepository.AddAsync(settings, cancellationToken);
        }

        // Update settings
        var updateResult = settings.Update(
            request.PushNotificationsEnabled,
            request.DarkModeEnabled,
            request.HapticFeedbackEnabled,
            request.LanguageCode);

        if (updateResult.IsFailure)
        {
            return Result.Failure<UserSettingsDto>(updateResult.Error);
        }

        // Update repository
        _settingsRepository.Update(settings);

        // Commit transaction
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Return updated settings
        var settingsDto = new UserSettingsDto(
            settings.PushNotificationsEnabled,
            settings.DarkModeEnabled,
            settings.HapticFeedbackEnabled,
            settings.LanguageCode,
            settings.UpdatedAt);

        return Result.Success(settingsDto);
    }
}
