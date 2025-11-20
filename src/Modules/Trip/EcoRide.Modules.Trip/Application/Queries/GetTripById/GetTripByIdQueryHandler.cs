using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Enums;
using EcoRide.Modules.Trip.Domain.Repositories;

namespace EcoRide.Modules.Trip.Application.Queries.GetTripById;

/// <summary>
/// Handler for getting trip details by ID
/// Implements US-007: Trip History - View trip details
/// </summary>
public sealed class GetTripByIdQueryHandler : IQueryHandler<GetTripByIdQuery, TripDetailsDto>
{
    private readonly IActiveTripRepository _tripRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IReceiptRepository _receiptRepository;

    public GetTripByIdQueryHandler(
        IActiveTripRepository tripRepository,
        IVehicleRepository vehicleRepository,
        IReceiptRepository receiptRepository)
    {
        _tripRepository = tripRepository;
        _vehicleRepository = vehicleRepository;
        _receiptRepository = receiptRepository;
    }

    public async Task<Result<TripDetailsDto>> Handle(
        GetTripByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get trip by ID
        var trip = await _tripRepository.GetByIdAsync(request.TripId, cancellationToken);

        if (trip is null)
        {
            return Result.Failure<TripDetailsDto>(
                new Error("Trip.NotFound", "Trip not found"));
        }

        // Authorization: Verify user owns the trip
        if (trip.UserId != request.UserId)
        {
            return Result.Failure<TripDetailsDto>(
                new Error("Trip.Unauthorized", "You are not authorized to view this trip"));
        }

        // Get vehicle information
        var vehicle = await _vehicleRepository.GetByIdAsync(trip.VehicleId, cancellationToken);
        var vehicleType = vehicle?.Type.ToString() ?? "Unknown";
        var vehicleCode = vehicle?.Code ?? "N/A";

        // Calculate distance
        var distanceMeters = trip.GetMockDistanceMeters();

        // Get location names (using coordinates for now)
        var startLocationName = $"{trip.StartLatitude:F4}, {trip.StartLongitude:F4}";
        var endLocationName = trip.EndLatitude.HasValue && trip.EndLongitude.HasValue
            ? $"{trip.EndLatitude.Value:F4}, {trip.EndLongitude.Value:F4}"
            : null;

        // Check if receipt exists
        var receipt = await _receiptRepository.GetByTripIdAsync(trip.Id, cancellationToken);
        var hasReceipt = receipt is not null;

        // Determine payment method
        var paymentMethod = receipt?.PaymentMethod ?? "N/A";

        // Map trip status
        var status = trip.Status == TripStatus.Completed ? "Completed" :
                     trip.Status == TripStatus.Active ? "Active" :
                     "Cancelled";

        // Calculate base cost and time cost from business rules (BR-004)
        var baseCost = Domain.Aggregates.ActiveTrip.BaseCostMAD;
        var timeCost = trip.DurationMinutes * Domain.Aggregates.ActiveTrip.PerMinuteRateMAD;

        return Result.Success(new TripDetailsDto(
            trip.Id,
            trip.UserId,
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
            baseCost,
            timeCost,
            trip.TotalCost,
            status,
            trip.RatingStars,
            trip.RatingComment,
            trip.RatedAt,
            paymentMethod,
            hasReceipt
        ));
    }
}
