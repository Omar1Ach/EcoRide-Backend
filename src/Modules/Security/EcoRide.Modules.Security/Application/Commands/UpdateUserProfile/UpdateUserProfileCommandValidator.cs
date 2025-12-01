using FluentValidation;

namespace EcoRide.Modules.Security.Application.Commands.UpdateUserProfile;

/// <summary>
/// Validator for UpdateUserProfileCommand
/// </summary>
public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+212[67]\d{8}$")
            .WithMessage("Invalid Moroccan phone number. Format: +212XXXXXXXXX");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");
    }
}
