using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Commands.EndTrip;

/// <summary>
/// Command to end an active trip
/// US-006: End Trip & Payment
/// </summary>
public sealed record EndTripCommand(
    Guid UserId,
    double EndLatitude,
    double EndLongitude) : ICommand<TripSummaryDto>;
