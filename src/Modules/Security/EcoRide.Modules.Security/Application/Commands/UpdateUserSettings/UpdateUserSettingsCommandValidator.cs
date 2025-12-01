using FluentValidation;

namespace EcoRide.Modules.Security.Application.Commands.UpdateUserSettings;

/// <summary>
/// Validator for UpdateUserSettingsCommand
/// </summary>
public sealed class UpdateUserSettingsCommandValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    private static readonly string[] SupportedLanguages = { "en", "fr", "ar", "es" };

    public UpdateUserSettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Language code is required")
            .Must(lang => SupportedLanguages.Contains(lang.ToLower()))
            .WithMessage("Language code must be one of: en, fr, ar, es");
    }
}
