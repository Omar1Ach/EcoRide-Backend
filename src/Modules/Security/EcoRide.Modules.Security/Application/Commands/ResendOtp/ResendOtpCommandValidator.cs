using FluentValidation;

namespace EcoRide.Modules.Security.Application.Commands.ResendOtp;

/// <summary>
/// Validator for ResendOtpCommand
/// </summary>
public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+212[67]\d{8}$")
            .WithMessage("Invalid Moroccan phone number");
    }
}
