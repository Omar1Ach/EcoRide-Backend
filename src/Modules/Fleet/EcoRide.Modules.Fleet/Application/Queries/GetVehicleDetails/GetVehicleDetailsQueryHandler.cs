using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Application.DTOs;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;

namespace EcoRide.Modules.Fleet.Application.Queries.GetVehicleDetails;

/// <summary>
/// Handler for GetVehicleDetailsQuery
/// </summary>
public sealed class GetVehicleDetailsQueryHandler : IQueryHandler<GetVehicleDetailsQuery, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository;

    private const int BasePriceCents = 500;
    private const int PricePerMinuteCents = 150;
    private const int EstimatedTripMinutes = 20;

    public GetVehicleDetailsQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<VehicleDto>> Handle(
        GetVehicleDetailsQuery request,
        CancellationToken cancellationToken)
    {
        // Get vehicle by ID
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);

        if (vehicle is null)
        {
            return Result.Failure<VehicleDto>(
                new Error("Vehicle.NotFound", "Vehicle not found"));
        }

        double? distance = null;
        string? distanceDisplay = null;

        // Calculate distance if user location provided
        if (request.UserLatitude.HasValue && request.UserLongitude.HasValue)
        {
            var userLocationResult = Location.Create(
                request.UserLatitude.Value,
                request.UserLongitude.Value);

            if (userLocationResult.IsSuccess)
            {
                distance = userLocationResult.Value.DistanceToInMeters(vehicle.Location);
                distanceDisplay = FormatDistance(distance.Value);
            }
        }

        var estimatedCost = CalculateEstimatedCost(EstimatedTripMinutes);

        var dto = new VehicleDto(
            vehicle.Id,
            vehicle.Code,
            vehicle.Type.ToString(),
            vehicle.Status.ToString(),
            vehicle.BatteryLevel.Value,
            vehicle.Location.Latitude,
            vehicle.Location.Longitude,
            distance,
            distanceDisplay,
            estimatedCost);

        return Result.Success(dto);
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
