using FluentValidation;

namespace EcoRide.Modules.Trip.Application.Commands.StartTrip;

/// <summary>
/// Validator for StartTripCommand
/// </summary>
public sealed class StartTripCommandValidator : AbstractValidator<StartTripCommand>
{
    public StartTripCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.QRCode)
            .NotEmpty()
            .WithMessage("QR code is required")
            .Matches(@"^ECO-\d{4}$")
            .WithMessage("QR code must be in format ECO-XXXX (e.g., ECO-1234)");

        RuleFor(x => x.StartLatitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.StartLongitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180");
    }
}
