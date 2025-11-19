using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Queries.GetTripHistory;

/// <summary>
/// Query to get trip history for a user
/// </summary>
public sealed record GetTripHistoryQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<GetTripHistoryResponse>;
