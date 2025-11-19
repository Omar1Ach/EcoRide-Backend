using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Repositories;
using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.Modules.Trip.Application.Commands.StartTrip;

/// <summary>
/// Handler for starting a trip via QR code scan
/// Implements US-004: Start Trip (QR Scan)
/// Test Scenarios: TC-030 to TC-034
/// </summary>
public sealed class StartTripCommandHandler : ICommandHandler<StartTripCommand, TripDto>
{
    private readonly IActiveTripRepository _tripRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartTripCommandHandler(
        IActiveTripRepository tripRepository,
        IReservationRepository reservationRepository,
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork)
    {
        _tripRepository = tripRepository;
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TripDto>> Handle(StartTripCommand request, CancellationToken cancellationToken)
    {
        // TC-030, TC-031: Validate QR code format
        var qrCodeResult = QRCode.Create(request.QRCode);
        if (qrCodeResult.IsFailure)
        {
            return Result.Failure<TripDto>(qrCodeResult.Error);
        }

        var qrCode = qrCodeResult.Value;

        // TC-030, TC-031: Find vehicle by QR code (use normalized value)
        var vehicle = await _vehicleRepository.GetByCodeAsync(qrCode.Value, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure<TripDto>(new Error(
                "Trip.VehicleNotFound",
                "Vehicle with this QR code not found"));
        }

        // Check if user already has an active trip
        var existingTrip = await _tripRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        if (existingTrip is not null)
        {
            return Result.Failure<TripDto>(new Error(
                "Trip.UserAlreadyHasActiveTrip",
                "You already have an active trip"));
        }

        // Check if vehicle is already in use
        var vehicleInUseTrip = await _tripRepository.GetActiveByVehicleIdAsync(vehicle.Id, cancellationToken);
        if (vehicleInUseTrip is not null)
        {
            return Result.Failure<TripDto>(new Error(
                "Trip.VehicleAlreadyInUse",
                "This vehicle is currently in use"));
        }

        // TC-032: Get user's active reservation and validate it matches the vehicle
        var reservation = await _reservationRepository.GetActiveReservationByUserIdAsync(
            request.UserId,
            cancellationToken);

        if (reservation is null || !reservation.IsActive())
        {
            return Result.Failure<TripDto>(new Error(
                "Trip.NoActiveReservation",
                "You must have an active reservation to start a trip"));
        }

        if (reservation.VehicleId != vehicle.Id)
        {
            return Result.Failure<TripDto>(new Error(
                "Trip.WrongVehicle",
                "This vehicle does not match your reservation"));
        }

        // Validate start location
        var startLocationResult = Location.Create(request.StartLatitude, request.StartLongitude);
        if (startLocationResult.IsFailure)
        {
            return Result.Failure<TripDto>(startLocationResult.Error);
        }

        // Start the trip
        var tripResult = ActiveTrip.Start(
            request.UserId,
            vehicle.Id,
            reservation.Id,
            startLocationResult.Value);

        if (tripResult.IsFailure)
        {
            return Result.Failure<TripDto>(tripResult.Error);
        }

        var trip = tripResult.Value;

        // Update vehicle status to InUse
        var startTripResult = vehicle.StartTrip();
        if (startTripResult.IsFailure)
        {
            return Result.Failure<TripDto>(startTripResult.Error);
        }

        // Convert reservation to trip
        var convertResult = reservation.ConvertToTrip();
        if (convertResult.IsFailure)
        {
            return Result.Failure<TripDto>(convertResult.Error);
        }

        // Persist changes
        await _tripRepository.AddAsync(trip, cancellationToken);
        _vehicleRepository.Update(vehicle);
        _reservationRepository.Update(reservation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return trip DTO
        return Result.Success(new TripDto(
            trip.Id,
            trip.UserId,
            trip.VehicleId,
            trip.ReservationId,
            trip.Status.ToString(),
            trip.StartTime,
            trip.StartLatitude,
            trip.StartLongitude,
            trip.EndTime,
            trip.EndLatitude,
            trip.EndLongitude,
            trip.TotalCost,
            trip.DurationMinutes,
            trip.GetCurrentEstimatedCost()));
    }
}
