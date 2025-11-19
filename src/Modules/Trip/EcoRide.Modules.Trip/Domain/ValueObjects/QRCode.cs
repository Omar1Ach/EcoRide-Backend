using EcoRide.BuildingBlocks.Domain;
using System.Text.RegularExpressions;

namespace EcoRide.Modules.Trip.Domain.ValueObjects;

/// <summary>
/// Value object representing vehicle QR code
/// Format: ECO-XXXX (e.g., ECO-1234)
/// </summary>
public sealed class QRCode : ValueObject
{
    private static readonly Regex QRCodePattern = new(@"^ECO-\d{4}$", RegexOptions.Compiled);

    public string Value { get; }

    private QRCode(string value)
    {
        Value = value;
    }

    public static Result<QRCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<QRCode>(new Error(
                "QRCode.Empty",
                "QR code cannot be empty"));
        }

        var normalizedValue = value.Trim().ToUpperInvariant();

        if (!QRCodePattern.IsMatch(normalizedValue))
        {
            return Result.Failure<QRCode>(new Error(
                "QRCode.InvalidFormat",
                "QR code must be in format ECO-XXXX (e.g., ECO-1234)"));
        }

        return Result.Success(new QRCode(normalizedValue));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
