namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// Trip summary after completion
/// US-006: End Trip & Payment
/// </summary>
public sealed record TripSummaryDto(
    Guid TripId,
    Guid UserId,
    Guid VehicleId,
    string VehicleCode,

    // Trip details
    DateTime StartTime,
    DateTime EndTime,
    int DurationMinutes,
    string DurationFormatted, // e.g., "18 minutes"

    // Distance (mock calculation)
    int DistanceMeters,
    string DistanceFormatted, // e.g., "2.8 km"

    // Cost breakdown (BR-004)
    decimal BaseCost,        // 5 MAD
    decimal TimeCost,        // 1.5 × minutes
    decimal TotalCost,       // Rounded total

    // Payment
    string PaymentStatus,    // e.g., "Paid from Wallet"
    decimal WalletBalanceBefore,
    decimal WalletBalanceAfter,

    // Location
    double StartLatitude,
    double StartLongitude,
    double EndLatitude,
    double EndLongitude);
