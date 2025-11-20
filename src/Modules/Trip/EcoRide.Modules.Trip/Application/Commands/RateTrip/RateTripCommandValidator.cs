using FluentValidation;

namespace EcoRide.Modules.Trip.Application.Commands.RateTrip;

/// <summary>
/// Validator for RateTripCommand
/// US-006: End Trip & Payment - Trip rating validation
/// </summary>
public sealed class RateTripCommandValidator : AbstractValidator<RateTripCommand>
{
    public RateTripCommandValidator()
    {
        RuleFor(x => x.TripId)
            .NotEmpty()
            .WithMessage("Trip ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Stars)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars");

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Comment))
            .WithMessage("Rating comment cannot exceed 500 characters");
    }
}
