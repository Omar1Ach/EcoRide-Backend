using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.ValueObjects;

/// <summary>
/// Payment method value object for storing credit card details
/// US-006: Credit card payment fallback
/// Note: Stores only last 4 digits for PCI compliance
/// </summary>
public sealed class PaymentMethod : ValueObject
{
    public Guid Id { get; }
    public string CardLast4 { get; }
    public string CardType { get; } // Visa, Mastercard, etc.
    public int ExpiryMonth { get; }
    public int ExpiryYear { get; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; }

    private PaymentMethod(
        Guid id,
        string cardLast4,
        string cardType,
        int expiryMonth,
        int expiryYear,
        bool isDefault)
    {
        Id = id;
        CardLast4 = cardLast4;
        CardType = cardType;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Create a new payment method with validation
    /// </summary>
    public static Result<PaymentMethod> Create(
        string cardLast4,
        string cardType,
        int expiryMonth,
        int expiryYear,
        bool isDefault = false)
    {
        // Validate last 4 digits
        if (string.IsNullOrWhiteSpace(cardLast4) || cardLast4.Length != 4 || !cardLast4.All(char.IsDigit))
        {
            return Result.Failure<PaymentMethod>(new Error(
                "PaymentMethod.InvalidCardLast4",
                "Card last 4 digits must be exactly 4 numeric digits"));
        }

        // Validate card type
        if (string.IsNullOrWhiteSpace(cardType))
        {
            return Result.Failure<PaymentMethod>(new Error(
                "PaymentMethod.InvalidCardType",
                "Card type is required"));
        }

        var validCardTypes = new[] { "Visa", "Mastercard", "Amex", "Discover" };
        if (!validCardTypes.Contains(cardType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<PaymentMethod>(new Error(
                "PaymentMethod.UnsupportedCardType",
                $"Card type must be one of: {string.Join(", ", validCardTypes)}"));
        }

        // Validate expiry date
        if (expiryMonth < 1 || expiryMonth > 12)
        {
            return Result.Failure<PaymentMethod>(new Error(
                "PaymentMethod.InvalidExpiryMonth",
                "Expiry month must be between 1 and 12"));
        }

        var currentYear = DateTime.UtcNow.Year;
        if (expiryYear < currentYear || expiryYear > currentYear + 20)
        {
            return Result.Failure<PaymentMethod>(new Error(
                "PaymentMethod.InvalidExpiryYear",
                $"Expiry year must be between {currentYear} and {currentYear + 20}"));
        }

        // Check if card is expired
        var expiryDate = new DateTime(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1);
        if (expiryDate < DateTime.UtcNow.Date)
        {
            return Result.Failure<PaymentMethod>(new Error(
                "PaymentMethod.CardExpired",
                "Card has already expired"));
        }

        return Result.Success(new PaymentMethod(
            Guid.NewGuid(),
            cardLast4,
            cardType,
            expiryMonth,
            expiryYear,
            isDefault));
    }

    /// <summary>
    /// Mark this payment method as default
    /// </summary>
    public void MarkAsDefault()
    {
        IsDefault = true;
    }

    /// <summary>
    /// Unmark this payment method as default
    /// </summary>
    public void UnmarkAsDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// Check if card is expired
    /// </summary>
    public bool IsExpired()
    {
        var expiryDate = new DateTime(ExpiryYear, ExpiryMonth, 1).AddMonths(1).AddDays(-1);
        return expiryDate < DateTime.UtcNow.Date;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return CardLast4;
        yield return CardType;
        yield return ExpiryMonth;
        yield return ExpiryYear;
    }

    public override string ToString()
    {
        return $"{CardType} ****{CardLast4}";
    }
}
