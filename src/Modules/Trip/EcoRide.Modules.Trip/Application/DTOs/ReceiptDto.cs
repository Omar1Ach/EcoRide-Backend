namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// DTO for trip receipt information
/// US-007: Trip History - View receipt
/// </summary>
public sealed record ReceiptDto(
    Guid ReceiptId,
    string ReceiptNumber,
    Guid TripId,
    Guid UserId,
    string VehicleCode,
    DateTime TripStartTime,
    DateTime TripEndTime,
    int DurationMinutes,
    int DistanceMeters,
    double StartLatitude,
    double StartLongitude,
    double EndLatitude,
    double EndLongitude,
    decimal BaseCostMAD,
    decimal TimeCostMAD,
    decimal TotalCostMAD,
    string PaymentMethod,
    string PaymentDetails,
    decimal WalletBalanceBefore,
    decimal WalletBalanceAfter,
    DateTime CreatedAt
);
