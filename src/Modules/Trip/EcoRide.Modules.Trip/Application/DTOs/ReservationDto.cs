namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// Data transfer object for reservation information
/// </summary>
public sealed class ReservationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid VehicleId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public int RemainingSeconds { get; init; }
    public bool IsActive { get; init; }
}
