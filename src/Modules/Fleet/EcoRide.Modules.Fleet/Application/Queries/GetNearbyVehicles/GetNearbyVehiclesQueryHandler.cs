using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Application.DTOs;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;

namespace EcoRide.Modules.Fleet.Application.Queries.GetNearbyVehicles;

/// <summary>
/// Handler for GetNearbyVehiclesQuery
/// </summary>
public sealed class GetNearbyVehiclesQueryHandler : IQueryHandler<GetNearbyVehiclesQuery, List<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;

    // Pricing: 5 MAD base + 1.5 MAD/min (as per migration)
    private const int BasePriceCents = 500; // 5 MAD
    private const int PricePerMinuteCents = 150; // 1.5 MAD
    private const int EstimatedTripMinutes = 20;

    public GetNearbyVehiclesQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<List<VehicleDto>>> Handle(
        GetNearbyVehiclesQuery request,
        CancellationToken cancellationToken)
    {
        // Create location value object
        var locationResult = Location.Create(request.Latitude, request.Longitude);
        if (locationResult.IsFailure)
        {
            return Result.Failure<List<VehicleDto>>(locationResult.Error);
        }

        var userLocation = locationResult.Value;

        // Get nearby vehicles from repository
        var vehicles = await _vehicleRepository.GetNearbyVehiclesAsync(
            userLocation,
            request.RadiusMeters,
            cancellationToken);

        // Map to DTOs with distance and cost calculations
        var vehicleDtos = vehicles
            .Select(v =>
            {
                var distance = userLocation.DistanceToInMeters(v.Location);
                var estimatedCost = CalculateEstimatedCost(EstimatedTripMinutes);

                return new VehicleDto(
                    v.Id,
                    v.Code,
                    v.Type.ToString(),
                    v.Status.ToString(),
                    v.BatteryLevel.Value,
                    v.Location.Latitude,
                    v.Location.Longitude,
                    distance,
                    FormatDistance(distance),
                    estimatedCost);
            })
            .OrderBy(v => v.DistanceInMeters) // Closest first
            .ToList();

        return Result.Success(vehicleDtos);
    }

    private static int CalculateEstimatedCost(int minutes)
    {
        return BasePriceCents + (PricePerMinuteCents * minutes);
    }

    private static string FormatDistance(double meters)
    {
        if (meters < 1000)
        {
            return $"{(int)meters}m away";
        }

        var km = meters / 1000.0;
        return $"{km:F1}km away";
    }
}
