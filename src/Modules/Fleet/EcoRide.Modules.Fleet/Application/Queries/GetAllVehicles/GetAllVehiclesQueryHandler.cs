using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Application.DTOs;
using EcoRide.Modules.Fleet.Domain.Repositories;

namespace EcoRide.Modules.Fleet.Application.Queries.GetAllVehicles;

/// <summary>
/// Handler for GetAllVehiclesQuery
/// </summary>
public sealed class GetAllVehiclesQueryHandler : IQueryHandler<GetAllVehiclesQuery, GetAllVehiclesResponse>
{
    private readonly IVehicleRepository _vehicleRepository;

    // Pricing: 5 MAD base + 1.5 MAD/min
    private const int BasePriceCents = 500; // 5 MAD
    private const int PricePerMinuteCents = 150; // 1.5 MAD
    private const int EstimatedTripMinutes = 20;

    public GetAllVehiclesQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<GetAllVehiclesResponse>> Handle(
        GetAllVehiclesQuery request,
        CancellationToken cancellationToken)
    {
        // Validate pagination
        if (request.PageNumber < 1)
        {
            return Result.Failure<GetAllVehiclesResponse>(
                new Error("Pagination.InvalidPageNumber", "Page number must be greater than 0"));
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<GetAllVehiclesResponse>(
                new Error("Pagination.InvalidPageSize", "Page size must be between 1 and 100"));
        }

        // Get vehicles from repository
        var (vehicles, totalCount) = await _vehicleRepository.GetAllAsync(
            request.Status,
            request.Type,
            request.MinBatteryLevel,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        // Map to DTOs
        var vehicleDtos = vehicles
            .Select(v =>
            {
                var estimatedCost = CalculateEstimatedCost(EstimatedTripMinutes);

                return new VehicleDto(
                    v.Id,
                    v.Code,
                    v.Type.ToString(),
                    v.Status.ToString(),
                    v.BatteryLevel.Value,
                    v.Location.Latitude,
                    v.Location.Longitude,
                    null, // No distance calculation for "get all"
                    null, // No distance string
                    estimatedCost);
            })
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new GetAllVehiclesResponse(
            vehicleDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Success(response);
    }

    private static int CalculateEstimatedCost(int minutes)
    {
        return BasePriceCents + (PricePerMinuteCents * minutes);
    }
}
