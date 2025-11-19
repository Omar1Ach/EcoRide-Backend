using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Commands.CreateReservation;

/// <summary>
/// Command to create a new reservation
/// BR-002: User can reserve only 1 vehicle at a time
/// </summary>
public sealed record CreateReservationCommand(
    Guid UserId,
    Guid VehicleId
) : ICommand<ReservationDto>;
