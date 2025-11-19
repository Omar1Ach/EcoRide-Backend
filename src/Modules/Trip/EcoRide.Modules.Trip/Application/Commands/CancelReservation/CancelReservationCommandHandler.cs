using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Domain.Repositories;

namespace EcoRide.Modules.Trip.Application.Commands.CancelReservation;

/// <summary>
/// Handler for cancelling reservations
/// BR-002: No penalty for manual cancellation
/// </summary>
public sealed class CancelReservationCommandHandler : ICommandHandler<CancelReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelReservationCommandHandler(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CancelReservationCommand request,
        CancellationToken cancellationToken)
    {
        // Get reservation
        var reservation = await _reservationRepository
            .GetByIdAsync(request.ReservationId, cancellationToken);

        if (reservation is null)
        {
            return Result.Failure(
                new Error("Reservation.NotFound", "Reservation not found"));
        }

        // Verify ownership
        if (reservation.UserId != request.UserId)
        {
            return Result.Failure(
                new Error("Reservation.Unauthorized", "You cannot cancel another user's reservation"));
        }

        // Cancel reservation
        var cancelResult = reservation.Cancel();

        if (cancelResult.IsFailure)
        {
            return cancelResult;
        }

        // Persist changes
        _reservationRepository.Update(reservation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
