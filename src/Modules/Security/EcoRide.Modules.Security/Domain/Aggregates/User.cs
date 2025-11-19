using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Domain.Enums;
using EcoRide.Modules.Security.Domain.Events;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Domain.Aggregates;

/// <summary>
/// User aggregate root managing registration and authentication
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    public Email Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public FullName FullName { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public bool IsActive { get; private set; }
    public bool PhoneVerified { get; private set; }
    public bool EmailVerified { get; private set; }
    public decimal WalletBalance { get; private set; } // BR-005: User wallet for payments
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Private constructor for EF Core
    private User() { }

    private User(
        Guid id,
        Email email,
        PhoneNumber phoneNumber,
        string passwordHash,
        FullName fullName)
    {
        Id = id;
        Email = email;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = UserRole.User;
        KycStatus = KycStatus.Pending;
        IsActive = true;
        PhoneVerified = false;
        EmailVerified = false;
        WalletBalance = 0m; // Start with zero balance
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new user pending registration
    /// </summary>
    public static Result<User> CreatePendingRegistration(
        Email email,
        PhoneNumber phoneNumber,
        string passwordHash,
        FullName fullName)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>(
                new Error("User.PasswordHashEmpty", "Password hash cannot be empty"));
        }

        var user = new User(
            Guid.NewGuid(),
            email,
            phoneNumber,
            passwordHash,
            fullName);

        user.AddDomainEvent(new UserRegisteredDomainEvent(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.FullName,
            user.CreatedAt));

        return Result.Success(user);
    }

    /// <summary>
    /// Verifies the user's phone number
    /// </summary>
    public Result VerifyPhone()
    {
        if (PhoneVerified)
        {
            return Result.Failure(
                new Error("User.PhoneAlreadyVerified", "Phone number is already verified"));
        }

        PhoneVerified = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OtpVerifiedDomainEvent(
            Id,
            PhoneNumber,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Updates the KYC status of the user
    /// </summary>
    public Result UpdateKycStatus(KycStatus newStatus)
    {
        if (KycStatus == newStatus)
        {
            return Result.Success();
        }

        KycStatus = newStatus;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Soft deletes the user account
    /// </summary>
    public Result Deactivate()
    {
        if (DeletedAt.HasValue)
        {
            return Result.Failure(
                new Error("User.AlreadyDeactivated", "User account is already deactivated"));
        }

        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Deduct amount from wallet (BR-005: Payment processing)
    /// </summary>
    public Result DeductFromWallet(decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure(
                new Error("User.InvalidAmount", "Amount must be greater than zero"));
        }

        if (WalletBalance < amount)
        {
            return Result.Failure(
                new Error("User.InsufficientFunds", $"Insufficient wallet balance. Available: {WalletBalance} MAD, Required: {amount} MAD"));
        }

        WalletBalance -= amount;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Add funds to wallet (BR-005: Wallet top-up)
    /// </summary>
    public Result AddToWallet(decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure(
                new Error("User.InvalidAmount", "Amount must be greater than zero"));
        }

        WalletBalance += amount;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
