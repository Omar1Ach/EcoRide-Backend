namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// DTO for detailed trip information
/// Includes all trip data plus rating information
/// US-007: Trip History - View trip details
/// </summary>
public sealed record TripDetailsDto(
    Guid TripId,
    Guid UserId,
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
    decimal BaseCostMAD,
    decimal TimeCostMAD,
    decimal TotalCostMAD,
    string Status,
    int? RatingStars,
    string? RatingComment,
    DateTime? RatedAt,
    string PaymentMethod,
    bool HasReceipt
);
