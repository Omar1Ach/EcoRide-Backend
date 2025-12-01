using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Queries.GetUserProfile;

/// <summary>
/// Query to get user profile information
/// </summary>
public sealed record GetUserProfileQuery(Guid UserId) : IQuery<UserProfileDto>;
