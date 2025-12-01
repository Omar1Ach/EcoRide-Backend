using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Queries.GetUserSettings;

/// <summary>
/// Query to get user settings
/// </summary>
public sealed record GetUserSettingsQuery(Guid UserId) : IQuery<UserSettingsDto>;
