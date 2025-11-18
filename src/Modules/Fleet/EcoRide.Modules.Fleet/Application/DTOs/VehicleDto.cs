namespace EcoRide.Modules.Fleet.Application.DTOs;

/// <summary>
/// Vehicle data transfer object
/// </summary>
public sealed record VehicleDto(
    Guid Id,
    string Code,
    string Type,
    string Status,
    int BatteryLevel,
    double Latitude,
    double Longitude,
    double? DistanceInMeters,
    string? DistanceDisplay,
    int? EstimatedCostCents
);
