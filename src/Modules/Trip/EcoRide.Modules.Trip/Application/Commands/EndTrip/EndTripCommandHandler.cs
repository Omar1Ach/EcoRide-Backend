using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Repositories;
using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.Modules.Trip.Application.Commands.EndTrip;

/// <summary>
/// Handler for ending an active trip and processing payment
/// US-006: End Trip & Payment
/// </summary>
public sealed class EndTripCommandHandler : ICommandHandler<EndTripCommand, TripSummaryDto>
{
    private readonly IActiveTripRepository _tripRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EndTripCommandHandler(
        IActiveTripRepository tripRepository,
        IVehicleRepository vehicleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _tripRepository = tripRepository;
        _vehicleRepository = vehicleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TripSummaryDto>> Handle(
        EndTripCommand request,
        CancellationToken cancellationToken)
    {
        // Get active trip
        var trip = await _tripRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        if (trip is null)
        {
            return Result.Failure<TripSummaryDto>(new Error(
                "Trip.NoActiveTrip",
                "No active trip found for this user"));
        }

        // Get vehicle
        var vehicle = await _vehicleRepository.GetByIdAsync(trip.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure<TripSummaryDto>(new Error(
                "Trip.VehicleNotFound",
                "Vehicle not found"));
        }

        // Get user for payment
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<TripSummaryDto>(new Error(
                "Trip.UserNotFound",
                "User not found"));
        }

        // Store wallet balance before payment
        var walletBalanceBefore = user.WalletBalance;

        // End the trip
        var endLocation = Location.Create(request.EndLatitude, request.EndLongitude);
        if (endLocation.IsFailure)
        {
            return Result.Failure<TripSummaryDto>(endLocation.Error);
        }

        var endResult = trip.End(endLocation.Value);
        if (endResult.IsFailure)
        {
            return Result.Failure<TripSummaryDto>(endResult.Error);
        }

        // Process payment (BR-005: Deduct from wallet)
        var paymentResult = user.DeductFromWallet(trip.TotalCost);
        if (paymentResult.IsFailure)
        {
            return Result.Failure<TripSummaryDto>(paymentResult.Error);
        }

        // Update vehicle location and status to Available
        var vehicleEndLocation = EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(
            request.EndLatitude,
            request.EndLongitude);

        if (vehicleEndLocation.IsFailure)
        {
            return Result.Failure<TripSummaryDto>(vehicleEndLocation.Error);
        }

        var endVehicleTripResult = vehicle.EndTrip(vehicleEndLocation.Value);
        if (endVehicleTripResult.IsFailure)
        {
            return Result.Failure<TripSummaryDto>(endVehicleTripResult.Error);
        }

        // Save all changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Calculate distance (mock)
        var distanceMeters = trip.DurationMinutes * ActiveTrip.MockDistanceMetersPerMinute;
        var distanceFormatted = distanceMeters >= 1000
            ? $"{distanceMeters / 1000.0:F1} km"
            : $"{distanceMeters} m";

        // Format duration
        var durationFormatted = trip.DurationMinutes == 1
            ? "1 minute"
            : $"{trip.DurationMinutes} minutes";

        // Calculate cost breakdown
        var baseCost = ActiveTrip.BaseCostMAD;
        var timeCost = trip.DurationMinutes * ActiveTrip.PerMinuteRateMAD;

        // Create trip summary
        var summary = new TripSummaryDto(
            trip.Id,
            trip.UserId,
            trip.VehicleId,
            vehicle.Code,
            trip.StartTime,
            trip.EndTime!.Value,
            trip.DurationMinutes,
            durationFormatted,
            distanceMeters,
            distanceFormatted,
            baseCost,
            timeCost,
            trip.TotalCost,
            "Paid from Wallet",
            walletBalanceBefore,
            user.WalletBalance,
            trip.StartLatitude,
            trip.StartLongitude,
            trip.EndLatitude!.Value,
            trip.EndLongitude!.Value);

        return Result.Success(summary);
    }
}
