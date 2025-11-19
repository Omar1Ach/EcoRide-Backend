using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Queries.GetActiveReservation;

/// <summary>
/// Query to get user's active reservation (if any)
/// Used for countdown timer display
/// </summary>
public sealed record GetActiveReservationQuery(
    Guid UserId
) : IQuery<ReservationDto?>;
