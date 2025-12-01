using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Commands.UpdateUserProfile;

/// <summary>
/// Command to update user profile
/// </summary>
public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FullName,
    string Email,
    string PhoneNumber
) : ICommand<UserProfileDto>;
