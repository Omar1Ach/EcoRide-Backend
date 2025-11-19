using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Repositories;

namespace EcoRide.Modules.Trip.Application.Commands.CreateReservation;

/// <summary>
/// Handler for creating vehicle reservations
/// Implements BR-002: User can reserve only 1 vehicle at a time
/// </summary>
public sealed class CreateReservationCommandHandler : ICommandHandler<CreateReservationCommand, ReservationDto>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationCommandHandler(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReservationDto>> Handle(
        CreateReservationCommand request,
        CancellationToken cancellationToken)
    {
        // BR-002: Check if user already has an active reservation
        var existingReservation = await _reservationRepository
            .GetActiveReservationByUserIdAsync(request.UserId, cancellationToken);

        if (existingReservation is not null && existingReservation.IsActive())
        {
            return Result.Failure<ReservationDto>(
                new Error(
                    "Reservation.UserAlreadyHasReservation",
                    "You already have an active reservation. Please cancel it first or wait for it to expire."));
        }

        // Check if vehicle is already reserved by another user
        var vehicleReservation = await _reservationRepository
            .GetActiveReservationByVehicleIdAsync(request.VehicleId, cancellationToken);

        if (vehicleReservation is not null && vehicleReservation.IsActive())
        {
            return Result.Failure<ReservationDto>(
                new Error(
                    "Reservation.VehicleAlreadyReserved",
                    "This vehicle is already reserved by another user."));
        }

        // Create new reservation
        var reservationResult = Reservation.Create(request.UserId, request.VehicleId);

        if (reservationResult.IsFailure)
        {
            return Result.Failure<ReservationDto>(reservationResult.Error);
        }

        var reservation = reservationResult.Value;

        // Persist to database
        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var dto = new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            VehicleId = reservation.VehicleId,
            Status = reservation.Status.ToString(),
            CreatedAt = reservation.CreatedAt,
            ExpiresAt = reservation.ExpiresAt,
            RemainingSeconds = reservation.GetRemainingSeconds(),
            IsActive = reservation.IsActive()
        };

        return Result.Success(dto);
    }
}
