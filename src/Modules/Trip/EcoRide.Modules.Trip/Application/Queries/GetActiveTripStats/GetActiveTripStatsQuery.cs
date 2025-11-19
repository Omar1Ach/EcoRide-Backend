using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Queries.GetActiveTripStats;

/// <summary>
/// Query to get active trip with real-time statistics
/// Implements US-005 acceptance criteria
/// </summary>
public sealed record GetActiveTripStatsQuery(Guid UserId) : IQuery<ActiveTripStatsDto>;
