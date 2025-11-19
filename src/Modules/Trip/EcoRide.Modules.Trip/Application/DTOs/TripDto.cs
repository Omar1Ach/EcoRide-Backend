namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// Data transfer object for active trip
/// </summary>
public sealed record TripDto(
    Guid Id,
    Guid UserId,
    Guid VehicleId,
    Guid? ReservationId,
    string Status,
    DateTime StartTime,
    double StartLatitude,
    double StartLongitude,
    DateTime? EndTime,
    double? EndLatitude,
    double? EndLongitude,
    decimal TotalCost,
    int DurationMinutes,
    decimal CurrentEstimatedCost);
