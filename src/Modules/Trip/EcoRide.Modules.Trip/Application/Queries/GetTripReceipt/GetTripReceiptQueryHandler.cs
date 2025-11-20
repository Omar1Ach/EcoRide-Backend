using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Domain.Repositories;

namespace EcoRide.Modules.Trip.Application.Queries.GetTripReceipt;

/// <summary>
/// Handler for getting trip receipt
/// Implements US-007: Trip History - View receipt
/// </summary>
public sealed class GetTripReceiptQueryHandler : IQueryHandler<GetTripReceiptQuery, ReceiptDto>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IActiveTripRepository _tripRepository;

    public GetTripReceiptQueryHandler(
        IReceiptRepository receiptRepository,
        IActiveTripRepository tripRepository)
    {
        _receiptRepository = receiptRepository;
        _tripRepository = tripRepository;
    }

    public async Task<Result<ReceiptDto>> Handle(
        GetTripReceiptQuery request,
        CancellationToken cancellationToken)
    {
        // Get trip to verify ownership
        var trip = await _tripRepository.GetByIdAsync(request.TripId, cancellationToken);

        if (trip is null)
        {
            return Result.Failure<ReceiptDto>(
                new Error("Trip.NotFound", "Trip not found"));
        }

        // Authorization: Verify user owns the trip
        if (trip.UserId != request.UserId)
        {
            return Result.Failure<ReceiptDto>(
                new Error("Receipt.Unauthorized", "You are not authorized to view this receipt"));
        }

        // Get receipt
        var receipt = await _receiptRepository.GetByTripIdAsync(request.TripId, cancellationToken);

        if (receipt is null)
        {
            return Result.Failure<ReceiptDto>(
                new Error("Receipt.NotFound", "Receipt not found for this trip"));
        }

        // Map to DTO
        return Result.Success(new ReceiptDto(
            receipt.Id,
            receipt.ReceiptNumber,
            receipt.TripId,
            receipt.UserId,
            receipt.VehicleCode,
            receipt.TripStartTime,
            receipt.TripEndTime,
            receipt.DurationMinutes,
            receipt.DistanceMeters,
            receipt.StartLatitude,
            receipt.StartLongitude,
            receipt.EndLatitude,
            receipt.EndLongitude,
            receipt.BaseCost,
            receipt.TimeCost,
            receipt.TotalCost,
            receipt.PaymentMethod,
            receipt.PaymentDetails,
            receipt.WalletBalanceBefore,
            receipt.WalletBalanceAfter,
            receipt.CreatedAt
        ));
    }
}
