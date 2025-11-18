using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.ValueObjects;

/// <summary>
/// Full name value object with validation
/// </summary>
public sealed class FullName : ValueObject
{
    private const int MinLength = 2;
    private const int MaxLength = 100;

    public string Value { get; }

    private FullName(string value)
    {
        Value = value;
    }

    public static Result<FullName> Create(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<FullName>(
                new Error("FullName.Empty", "Full name cannot be empty"));
        }

        fullName = fullName.Trim();

        if (fullName.Length < MinLength)
        {
            return Result.Failure<FullName>(
                new Error("FullName.TooShort", $"Full name must be at least {MinLength} characters"));
        }

        if (fullName.Length > MaxLength)
        {
            return Result.Failure<FullName>(
                new Error("FullName.TooLong", $"Full name cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new FullName(fullName));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(FullName name) => name.Value;
}
