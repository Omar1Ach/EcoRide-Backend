using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Queries.GetTripById;

/// <summary>
/// Query to get trip details by ID
/// US-007: Trip History - View trip details
/// </summary>
public sealed record GetTripByIdQuery(Guid TripId, Guid UserId) : IQuery<TripDetailsDto>;
