namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// Real-time active trip statistics for live display
/// US-005: Active Trip Tracking
/// </summary>
public sealed record ActiveTripStatsDto(
    Guid TripId,
    Guid UserId,
    Guid VehicleId,
    string VehicleCode,

    // Timer display (MM:SS format)
    int DurationSeconds,
    string DurationFormatted, // e.g., "15:34"

    // Cost tracking (BR-004: 5 + 1.5 × minutes, rounded)
    decimal CurrentCost,
    decimal BaseCost,
    decimal PerMinuteRate,

    // Distance tracking (mock: +100m/minute)
    int DistanceMeters,
    string DistanceFormatted, // e.g., "1.5 km"

    // Vehicle info
    int BatteryPercentage,
    bool IsLowBattery, // Warning if < 10%

    // Trip metadata
    DateTime StartTime,
    double StartLatitude,
    double StartLongitude);
