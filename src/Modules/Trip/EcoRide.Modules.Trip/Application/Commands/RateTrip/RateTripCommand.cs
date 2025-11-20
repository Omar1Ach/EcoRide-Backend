using EcoRide.BuildingBlocks.Application.Messaging;

namespace EcoRide.Modules.Trip.Application.Commands.RateTrip;

/// <summary>
/// Command to rate a completed trip
/// US-006: End Trip & Payment - Trip rating feature
/// </summary>
public sealed record RateTripCommand(
    Guid TripId,
    Guid UserId,
    int Stars,
    string? Comment = null) : ICommand;
