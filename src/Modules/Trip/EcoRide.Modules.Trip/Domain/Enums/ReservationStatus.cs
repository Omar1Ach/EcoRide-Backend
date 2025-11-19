namespace EcoRide.Modules.Trip.Domain.Enums;

/// <summary>
/// Reservation lifecycle states
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Reservation is currently active (within 5-minute window)
    /// </summary>
    Active,

    /// <summary>
    /// User manually cancelled the reservation
    /// </summary>
    Cancelled,

    /// <summary>
    /// Reservation expired after 5 minutes without conversion to trip
    /// </summary>
    Expired,

    /// <summary>
    /// Reservation was successfully converted to an active trip
    /// </summary>
    Converted
}
