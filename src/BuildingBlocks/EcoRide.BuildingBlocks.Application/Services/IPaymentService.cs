using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.BuildingBlocks.Application.Services;

/// <summary>
/// Payment service interface for processing trip payments
/// US-006: Credit card payment fallback and retry logic
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Process payment for a trip with wallet and credit card fallback
    /// Implements retry logic with exponential backoff
    /// </summary>
    Task<Result<PaymentResult>> ProcessTripPaymentAsync(
        Guid userId,
        decimal amount,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of payment processing
/// </summary>
public sealed record PaymentResult(
    PaymentMethod Method,
    string Message,
    decimal AmountCharged,
    string? CardLast4 = null);

/// <summary>
/// Payment method used
/// </summary>
public enum PaymentMethod
{
    Wallet,
    CreditCard
}
