using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Commands.StartTrip;

/// <summary>
/// Command to start a trip by scanning QR code
/// Implements US-004 acceptance criteria
/// </summary>
public sealed record StartTripCommand(
    Guid UserId,
    string QRCode,
    double StartLatitude,
    double StartLongitude) : ICommand<TripDto>;
