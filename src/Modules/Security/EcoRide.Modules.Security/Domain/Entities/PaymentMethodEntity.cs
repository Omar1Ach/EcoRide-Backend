using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.Entities;

/// <summary>
/// Payment method entity for storing user credit cards
/// US-006: Credit card payment fallback
/// Stores only last 4 digits for PCI compliance
/// </summary>
public sealed class PaymentMethodEntity : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string CardLast4 { get; private set; } = null!;
    public string CardType { get; private set; } = null!;
    public int ExpiryMonth { get; private set; }
    public int ExpiryYear { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private PaymentMethodEntity() { }

    private PaymentMethodEntity(
        Guid id,
        Guid userId,
        string cardLast4,
        string cardType,
        int expiryMonth,
        int expiryYear,
        bool isDefault)
    {
        Id = id;
        UserId = userId;
        CardLast4 = cardLast4;
        CardType = cardType;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<PaymentMethodEntity> Create(
        Guid userId,
        string cardLast4,
        string cardType,
        int expiryMonth,
        int expiryYear,
        bool isDefault = false)
    {
        // Validate last 4 digits
        if (string.IsNullOrWhiteSpace(cardLast4) || cardLast4.Length != 4 || !cardLast4.All(char.IsDigit))
        {
            return Result.Failure<PaymentMethodEntity>(new Error(
                "PaymentMethod.InvalidCardLast4",
                "Card last 4 digits must be exactly 4 numeric digits"));
        }

        // Validate card type
        if (string.IsNullOrWhiteSpace(cardType))
        {
            return Result.Failure<PaymentMethodEntity>(new Error(
                "PaymentMethod.InvalidCardType",
                "Card type is required"));
        }

        var validCardTypes = new[] { "Visa", "Mastercard", "Amex", "Discover" };
        if (!validCardTypes.Contains(cardType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<PaymentMethodEntity>(new Error(
                "PaymentMethod.UnsupportedCardType",
                $"Card type must be one of: {string.Join(", ", validCardTypes)}"));
        }

        // Validate expiry date
        if (expiryMonth < 1 || expiryMonth > 12)
        {
            return Result.Failure<PaymentMethodEntity>(new Error(
                "PaymentMethod.InvalidExpiryMonth",
                "Expiry month must be between 1 and 12"));
        }

        var currentYear = DateTime.UtcNow.Year;
        if (expiryYear < currentYear || expiryYear > currentYear + 20)
        {
            return Result.Failure<PaymentMethodEntity>(new Error(
                "PaymentMethod.InvalidExpiryYear",
                $"Expiry year must be between {currentYear} and {currentYear + 20}"));
        }

        // Check if card is expired
        var expiryDate = new DateTime(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1);
        if (expiryDate < DateTime.UtcNow.Date)
        {
            return Result.Failure<PaymentMethodEntity>(new Error(
                "PaymentMethod.CardExpired",
                "Card has already expired"));
        }

        return Result.Success(new PaymentMethodEntity(
            Guid.NewGuid(),
            userId,
            cardLast4,
            cardType,
            expiryMonth,
            expiryYear,
            isDefault));
    }

    public void MarkAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnmarkAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        var expiryDate = new DateTime(ExpiryYear, ExpiryMonth, 1).AddMonths(1).AddDays(-1);
        return expiryDate < DateTime.UtcNow.Date;
    }

    public override string ToString()
    {
        return $"{CardType} ****{CardLast4}";
    }
}
