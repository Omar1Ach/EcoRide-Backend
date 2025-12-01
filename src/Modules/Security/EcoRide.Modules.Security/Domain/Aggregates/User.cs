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

    // Security fields
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

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
    /// Update user profile information
    /// </summary>
    public Result UpdateProfile(FullName fullName, Email email, PhoneNumber phoneNumber)
    {
        if (fullName == null)
        {
            return Result.Failure(
                new Error("User.InvalidFullName", "Full name is required"));
        }

        if (email == null)
        {
            return Result.Failure(
                new Error("User.InvalidEmail", "Email is required"));
        }

        if (phoneNumber == null)
        {
            return Result.Failure(
                new Error("User.InvalidPhoneNumber", "Phone number is required"));
        }

        // If email or phone changed, reset verification status
        bool emailChanged = Email.Value != email.Value;
        bool phoneChanged = PhoneNumber.Value != phoneNumber.Value;

        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;

        if (emailChanged)
        {
            EmailVerified = false;
        }

        if (phoneChanged)
        {
            PhoneVerified = false;
        }

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

    // ========================================
    // Security Methods
    // ========================================

    /// <summary>
    /// Check if account is locked out
    /// </summary>
    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Record failed login attempt and lock account if threshold exceeded
    /// </summary>
    public Result RecordFailedLogin()
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        // Lock account after 5 failed attempts for 15 minutes
        if (FailedLoginAttempts >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            return Result.Failure(
                new Error("User.AccountLocked",
                    $"Account locked due to multiple failed login attempts. Try again after {LockoutEnd:yyyy-MM-dd HH:mm:ss} UTC"));
        }

        return Result.Success();
    }

    /// <summary>
    /// Reset failed login attempts on successful login
    /// </summary>
    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set refresh token
    /// </summary>
    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validate refresh token
    /// </summary>
    public Result ValidateRefreshToken(string token)
    {
        if (string.IsNullOrWhiteSpace(RefreshToken))
        {
            return Result.Failure(
                new Error("User.NoRefreshToken", "No refresh token found"));
        }

        if (RefreshToken != token)
        {
            return Result.Failure(
                new Error("User.InvalidRefreshToken", "Invalid refresh token"));
        }

        if (!RefreshTokenExpiryTime.HasValue || RefreshTokenExpiryTime.Value < DateTime.UtcNow)
        {
            return Result.Failure(
                new Error("User.RefreshTokenExpired", "Refresh token has expired"));
        }

        return Result.Success();
    }

    /// <summary>
    /// Revoke refresh token
    /// </summary>
    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generate password reset token
    /// </summary>
    public string GeneratePasswordResetToken()
    {
        PasswordResetToken = Guid.NewGuid().ToString("N");
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Valid for 1 hour
        UpdatedAt = DateTime.UtcNow;
        return PasswordResetToken;
    }

    /// <summary>
    /// Validate password reset token
    /// </summary>
    public Result ValidatePasswordResetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(PasswordResetToken))
        {
            return Result.Failure(
                new Error("User.NoResetToken", "No password reset token found"));
        }

        if (PasswordResetToken != token)
        {
            return Result.Failure(
                new Error("User.InvalidResetToken", "Invalid password reset token"));
        }

        if (!PasswordResetTokenExpiry.HasValue || PasswordResetTokenExpiry.Value < DateTime.UtcNow)
        {
            return Result.Failure(
                new Error("User.ResetTokenExpired", "Password reset token has expired"));
        }

        return Result.Success();
    }

    /// <summary>
    /// Reset password
    /// </summary>
    public Result ResetPassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            return Result.Failure(
                new Error("User.InvalidPasswordHash", "Password hash cannot be empty"));
        }

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        FailedLoginAttempts = 0; // Reset failed attempts on password reset
        LockoutEnd = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
