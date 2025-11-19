namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// Response DTO for trip history query
/// </summary>
public sealed record GetTripHistoryResponse(
    List<TripHistoryDto> Trips,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
