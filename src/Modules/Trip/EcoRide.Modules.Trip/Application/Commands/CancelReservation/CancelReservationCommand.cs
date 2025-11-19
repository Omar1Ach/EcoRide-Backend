using EcoRide.BuildingBlocks.Application.Messaging;

namespace EcoRide.Modules.Trip.Application.Commands.CancelReservation;

/// <summary>
/// Command to cancel an active reservation
/// BR-002: No penalty for manual cancellation
/// </summary>
public sealed record CancelReservationCommand(
    Guid ReservationId,
    Guid UserId
) : ICommand;
