using FluentValidation;

namespace EcoRide.Modules.Security.Application.Commands.VerifyOtp;

/// <summary>
/// Validator for VerifyOtpCommand
/// </summary>
public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+212[67]\d{8}$")
            .WithMessage("Invalid Moroccan phone number");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("OTP code is required")
            .Length(6).WithMessage("OTP code must be exactly 6 digits")
            .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits");
    }
}
