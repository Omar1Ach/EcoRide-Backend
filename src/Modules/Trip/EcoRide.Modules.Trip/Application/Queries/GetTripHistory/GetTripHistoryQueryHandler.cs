using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Repositories;

namespace EcoRide.Modules.Trip.Application.Queries.GetTripHistory;

/// <summary>
/// Handler for getting user's trip history
/// Implements US-007: Trip History
/// Test Scenarios: TC-060 to TC-064
/// </summary>
public sealed class GetTripHistoryQueryHandler : IQueryHandler<GetTripHistoryQuery, GetTripHistoryResponse>
{
    private readonly IActiveTripRepository _tripRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public GetTripHistoryQueryHandler(
        IActiveTripRepository tripRepository,
        IVehicleRepository vehicleRepository)
    {
        _tripRepository = tripRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<GetTripHistoryResponse>> Handle(
        GetTripHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        if (request.PageNumber < 1)
        {
            return Result.Failure<GetTripHistoryResponse>(
                new Error("Pagination.InvalidPageNumber", "Page number must be greater than 0"));
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<GetTripHistoryResponse>(
                new Error("Pagination.InvalidPageSize", "Page size must be between 1 and 100"));
        }

        // Get trip history with pagination (sorted by date, newest first)
        var (trips, totalCount) = await _tripRepository.GetTripHistoryAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        // Get unique vehicle IDs to fetch vehicle details
        var vehicleIds = trips.Select(t => t.VehicleId).Distinct().ToList();

        // Fetch all vehicles in one query for efficiency
        var vehicles = new Dictionary<Guid, EcoRide.Modules.Fleet.Domain.Aggregates.Vehicle>();
        foreach (var vehicleId in vehicleIds)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
            if (vehicle is not null)
            {
                vehicles[vehicleId] = vehicle;
            }
        }

        // Map to DTOs with vehicle information
        var tripDtos = trips.Select(trip =>
        {
            var vehicle = vehicles.GetValueOrDefault(trip.VehicleId);
            var vehicleType = vehicle?.Type.ToString() ?? "Unknown";
            var vehicleCode = vehicle?.Code ?? "N/A";

            // Calculate distance using mock formula (100m per minute)
            var distanceMeters = trip.GetMockDistanceMeters();

            // For location names, we'll use coordinate strings for now
            // In a real app, you'd use a geocoding service
            var startLocationName = $"{trip.StartLatitude:F4}, {trip.StartLongitude:F4}";
            var endLocationName = trip.EndLatitude.HasValue && trip.EndLongitude.HasValue
                ? $"{trip.EndLatitude.Value:F4}, {trip.EndLongitude.Value:F4}"
                : null;

            return new TripHistoryDto(
                trip.Id,
                vehicleType,
                vehicleCode,
                trip.StartTime,
                trip.EndTime,
                startLocationName,
                trip.StartLatitude,
                trip.StartLongitude,
                endLocationName,
                trip.EndLatitude,
                trip.EndLongitude,
                trip.DurationMinutes,
                distanceMeters,
                trip.TotalCost
            );
        }).ToList();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return Result.Success(new GetTripHistoryResponse(
            tripDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages
        ));
    }
}
