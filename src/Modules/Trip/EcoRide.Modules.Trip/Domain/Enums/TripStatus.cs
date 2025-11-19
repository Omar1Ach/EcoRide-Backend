namespace EcoRide.Modules.Trip.Domain.Enums;

/// <summary>
/// Status of trip lifecycle
/// </summary>
public enum TripStatus
{
    Active,      // Trip is ongoing
    Completed,   // Trip ended normally
    Cancelled    // Trip was cancelled
}
