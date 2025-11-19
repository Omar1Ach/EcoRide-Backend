using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Repositories;

namespace EcoRide.Modules.Trip.Application.Queries.GetActiveReservation;

/// <summary>
/// Handler for getting user's active reservation
/// </summary>
public sealed class GetActiveReservationQueryHandler
    : IQueryHandler<GetActiveReservationQuery, ReservationDto?>
{
    private readonly IReservationRepository _reservationRepository;

    public GetActiveReservationQueryHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<ReservationDto?>> Handle(
        GetActiveReservationQuery request,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository
            .GetActiveReservationByUserIdAsync(request.UserId, cancellationToken);

        if (reservation is null || !reservation.IsActive())
        {
            return Result.Success<ReservationDto?>(null);
        }

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

        return Result.Success<ReservationDto?>(dto);
    }
}
