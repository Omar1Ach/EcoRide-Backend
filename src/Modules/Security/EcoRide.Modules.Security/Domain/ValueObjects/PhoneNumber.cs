using EcoRide.BuildingBlocks.Domain;
using System.Text.RegularExpressions;

namespace EcoRide.Modules.Security.Domain.ValueObjects;

/// <summary>
/// Moroccan phone number value object (+212XXXXXXXXX)
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+212[67]\d{8}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.Empty", "Phone number cannot be empty"));
        }

        phoneNumber = phoneNumber.Trim();

        if (!PhoneRegex.IsMatch(phoneNumber))
        {
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.Invalid", "Invalid Moroccan phone number. Must be +212XXXXXXXXX with mobile prefix (6 or 7)"));
        }

        return Result.Success(new PhoneNumber(phoneNumber));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
