namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// DTO for trip history item
/// </summary>
public sealed record TripHistoryDto(
    Guid TripId,
    string VehicleType,
    string VehicleCode,
    DateTime StartedAt,
    DateTime? EndedAt,
    string StartLocationName,
    double StartLatitude,
    double StartLongitude,
    string? EndLocationName,
    double? EndLatitude,
    double? EndLongitude,
    int DurationMinutes,
    int DistanceMeters,
    decimal CostMAD
);
