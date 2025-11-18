using EcoRide.BuildingBlocks.Domain;
using System.Text.RegularExpressions;

namespace EcoRide.Modules.Security.Domain.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public sealed class Email : ValueObject
{
    private const int MaxLength = 255;
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(
                new Error("Email.Empty", "Email cannot be empty"));
        }

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
        {
            return Result.Failure<Email>(
                new Error("Email.TooLong", $"Email cannot exceed {MaxLength} characters"));
        }

        if (!EmailRegex.IsMatch(email))
        {
            return Result.Failure<Email>(
                new Error("Email.Invalid", "Invalid email format"));
        }

        return Result.Success(new Email(email));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
