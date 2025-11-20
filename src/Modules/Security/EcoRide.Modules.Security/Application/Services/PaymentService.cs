using EcoRide.BuildingBlocks.Application.Services;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EcoRide.Modules.Security.Application.Services;

/// <summary>
/// Payment service implementation with retry logic and fallback
/// US-006: Credit card payment fallback and retry logic (TC-054)
/// </summary>
public sealed class PaymentService : IPaymentService
{
    private readonly IUserRepository _userRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ILogger<PaymentService> _logger;

    private const int MaxRetries = 3;
    private const int InitialDelayMs = 500;

    public PaymentService(
        IUserRepository userRepository,
        IPaymentMethodRepository paymentMethodRepository,
        ILogger<PaymentService> logger)
    {
        _userRepository = userRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _logger = logger;
    }

    public async Task<Result<PaymentResult>> ProcessTripPaymentAsync(
        Guid userId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return Result.Failure<PaymentResult>(new Error(
                "Payment.InvalidAmount",
                "Payment amount must be greater than zero"));
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<PaymentResult>(new Error(
                "Payment.UserNotFound",
                "User not found"));
        }

        // Try wallet payment first (BR-005)
        if (user.WalletBalance >= amount)
        {
            return await ProcessWalletPaymentWithRetryAsync(user, amount, cancellationToken);
        }

        _logger.LogWarning(
            "Insufficient wallet balance for user {UserId}. Balance: {Balance} MAD, Required: {Amount} MAD. Attempting credit card fallback.",
            userId, user.WalletBalance, amount);

        // Wallet insufficient - try credit card fallback
        return await ProcessCreditCardPaymentWithRetryAsync(userId, amount, cancellationToken);
    }

    private async Task<Result<PaymentResult>> ProcessWalletPaymentWithRetryAsync(
        Domain.Aggregates.User user,
        decimal amount,
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var deductResult = user.DeductFromWallet(amount);
                if (deductResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Wallet payment successful for user {UserId}. Amount: {Amount} MAD",
                        user.Id, amount);

                    return Result.Success(new PaymentResult(
                        PaymentMethod.Wallet,
                        "Paid from Wallet",
                        amount));
                }

                // If deduction failed, return failure (don't retry insufficient funds)
                return Result.Failure<PaymentResult>(deductResult.Error);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Wallet payment attempt {Attempt} failed for user {UserId}",
                    attempt, user.Id);

                if (attempt == MaxRetries)
                {
                    return Result.Failure<PaymentResult>(new Error(
                        "Payment.WalletPaymentFailed",
                        $"Wallet payment failed after {MaxRetries} attempts: {ex.Message}"));
                }

                // Exponential backoff
                await Task.Delay(InitialDelayMs * (int)Math.Pow(2, attempt - 1), cancellationToken);
            }
        }

        return Result.Failure<PaymentResult>(new Error(
            "Payment.WalletPaymentFailed",
            "Wallet payment failed after all retry attempts"));
    }

    private async Task<Result<PaymentResult>> ProcessCreditCardPaymentWithRetryAsync(
        Guid userId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        // Get default payment method
        var paymentMethod = await _paymentMethodRepository.GetDefaultByUserIdAsync(userId, cancellationToken);
        if (paymentMethod is null)
        {
            return Result.Failure<PaymentResult>(new Error(
                "Payment.NoPaymentMethod",
                "No default payment method found. Please add a credit card."));
        }

        // Check if card is expired
        if (paymentMethod.IsExpired())
        {
            return Result.Failure<PaymentResult>(new Error(
                "Payment.CardExpired",
                "Credit card has expired. Please update your payment method."));
        }

        // Simulate credit card payment with retry logic (TC-054)
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    "Attempting credit card payment for user {UserId}. Attempt {Attempt}/{MaxRetries}. Amount: {Amount} MAD",
                    userId, attempt, MaxRetries, amount);

                // Simulate payment gateway call
                var paymentSuccess = await SimulateCreditCardPaymentAsync(
                    paymentMethod.CardLast4,
                    paymentMethod.CardType,
                    amount,
                    cancellationToken);

                if (paymentSuccess)
                {
                    _logger.LogInformation(
                        "Credit card payment successful for user {UserId}. Card: {CardType} ****{Last4}. Amount: {Amount} MAD",
                        userId, paymentMethod.CardType, paymentMethod.CardLast4, amount);

                    return Result.Success(new PaymentResult(
                        PaymentMethod.CreditCard,
                        $"Paid with {paymentMethod.CardType} ****{paymentMethod.CardLast4}",
                        amount,
                        paymentMethod.CardLast4));
                }

                _logger.LogWarning(
                    "Credit card payment attempt {Attempt} failed for user {UserId}",
                    attempt, userId);

                if (attempt < MaxRetries)
                {
                    // Exponential backoff
                    var delay = InitialDelayMs * (int)Math.Pow(2, attempt - 1);
                    _logger.LogInformation(
                        "Retrying credit card payment after {Delay}ms delay", delay);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Credit card payment attempt {Attempt} encountered error for user {UserId}",
                    attempt, userId);

                if (attempt == MaxRetries)
                {
                    return Result.Failure<PaymentResult>(new Error(
                        "Payment.CreditCardPaymentFailed",
                        $"Credit card payment failed after {MaxRetries} attempts: {ex.Message}"));
                }

                // Exponential backoff
                await Task.Delay(InitialDelayMs * (int)Math.Pow(2, attempt - 1), cancellationToken);
            }
        }

        return Result.Failure<PaymentResult>(new Error(
            "Payment.CreditCardPaymentFailed",
            $"Credit card payment failed after {MaxRetries} retry attempts. Please try again later or contact support."));
    }

    /// <summary>
    /// Simulate credit card payment (mock payment gateway)
    /// In production, this would call a real payment gateway like Stripe, PayPal, etc.
    /// </summary>
    private async Task<bool> SimulateCreditCardPaymentAsync(
        string cardLast4,
        string cardType,
        decimal amount,
        CancellationToken cancellationToken)
    {
        // Simulate network delay
        await Task.Delay(100, cancellationToken);

        // Simulate 90% success rate for testing
        // In production, this would be a real payment gateway call
        return true; // Always succeed in this mock implementation
    }
}
