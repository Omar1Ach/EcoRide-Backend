using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Repositories;

namespace EcoRide.Modules.Trip.Application.Queries.GetActiveTripStats;

/// <summary>
/// Handler for getting active trip statistics
/// Implements US-005: Real-time trip tracking
/// </summary>
public sealed class GetActiveTripStatsQueryHandler : IQueryHandler<GetActiveTripStatsQuery, ActiveTripStatsDto>
{
    private readonly IActiveTripRepository _tripRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public GetActiveTripStatsQueryHandler(
        IActiveTripRepository tripRepository,
        IVehicleRepository vehicleRepository)
    {
        _tripRepository = tripRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<ActiveTripStatsDto>> Handle(
        GetActiveTripStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Get active trip
        var trip = await _tripRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        if (trip is null)
        {
            return Result.Failure<ActiveTripStatsDto>(new Error(
                "Trip.NoActiveTrip",
                "No active trip found for this user"));
        }

        // Get vehicle for battery info and code
        var vehicle = await _vehicleRepository.GetByIdAsync(trip.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure<ActiveTripStatsDto>(new Error(
                "Trip.VehicleNotFound",
                "Vehicle not found"));
        }

        // Calculate real-time stats
        var durationMinutes = trip.GetCurrentDurationMinutes();
        var durationSeconds = (int)(DateTime.UtcNow - trip.StartTime).TotalSeconds;
        var currentCost = trip.GetCurrentEstimatedCost();
        var distanceMeters = trip.GetMockDistanceMeters();

        // Format duration as MM:SS
        var minutes = durationSeconds / 60;
        var seconds = durationSeconds % 60;
        var durationFormatted = $"{minutes:D2}:{seconds:D2}";

        // Format distance
        var distanceFormatted = distanceMeters >= 1000
            ? $"{distanceMeters / 1000.0:F1} km"
            : $"{distanceMeters} m";

        // Battery warning (BR-004: < 10%)
        var batteryPercentage = vehicle.BatteryLevel.Value;
        var isLowBattery = batteryPercentage < 10;

        var dto = new ActiveTripStatsDto(
            trip.Id,
            trip.UserId,
            trip.VehicleId,
            vehicle.Code,
            durationSeconds,
            durationFormatted,
            currentCost,
            ActiveTrip.BaseCostMAD,
            ActiveTrip.PerMinuteRateMAD,
            distanceMeters,
            distanceFormatted,
            batteryPercentage,
            isLowBattery,
            trip.StartTime,
            trip.StartLatitude,
            trip.StartLongitude);

        return Result.Success(dto);
    }
}
