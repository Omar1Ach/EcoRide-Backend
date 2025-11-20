using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Trip.Domain.Repositories;
using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.Modules.Trip.Application.Commands.RateTrip;

/// <summary>
/// Handler for rating a completed trip
/// US-006: End Trip & Payment - Trip rating feature
/// </summary>
public sealed class RateTripCommandHandler : ICommandHandler<RateTripCommand>
{
    private readonly IActiveTripRepository _tripRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RateTripCommandHandler(
        IActiveTripRepository tripRepository,
        IUnitOfWork unitOfWork)
    {
        _tripRepository = tripRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RateTripCommand request,
        CancellationToken cancellationToken)
    {
        // Get the trip
        var trip = await _tripRepository.GetByIdAsync(request.TripId, cancellationToken);
        if (trip is null)
        {
            return Result.Failure(new Error(
                "Trip.NotFound",
                "Trip not found"));
        }

        // Verify ownership
        if (trip.UserId != request.UserId)
        {
            return Result.Failure(new Error(
                "Trip.Unauthorized",
                "You can only rate your own trips"));
        }

        // Create rating value object
        var ratingResult = Rating.Create(request.Stars, request.Comment);
        if (ratingResult.IsFailure)
        {
            return Result.Failure(ratingResult.Error);
        }

        // Add rating to trip
        var addRatingResult = trip.AddRating(ratingResult.Value);
        if (addRatingResult.IsFailure)
        {
            return Result.Failure(addRatingResult.Error);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
