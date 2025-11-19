using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Enums;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.UnitTests.Security.Domain;

public class UserTests
{
    [Fact]
    public void CreatePendingRegistration_WithValidData_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;

        // Act
        var result = User.CreatePendingRegistration(email, phone, passwordHash, fullName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(phone, result.Value.PhoneNumber);
        Assert.Equal(passwordHash, result.Value.PasswordHash);
        Assert.Equal(fullName, result.Value.FullName);
        Assert.Equal(UserRole.User, result.Value.Role);
        Assert.Equal(KycStatus.Pending, result.Value.KycStatus);
        Assert.False(result.Value.PhoneVerified);
        Assert.False(result.Value.EmailVerified);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.DomainEvents);
    }

    [Fact]
    public void CreatePendingRegistration_WithEmptyPasswordHash_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "";
        var fullName = FullName.Create("John Doe").Value;

        // Act
        var result = User.CreatePendingRegistration(email, phone, passwordHash, fullName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.PasswordHashEmpty", result.Error.Code);
    }

    [Fact]
    public void VerifyPhone_WhenNotVerified_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var result = user.VerifyPhone();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.PhoneVerified);
        Assert.Equal(2, user.DomainEvents.Count);
    }

    [Fact]
    public void VerifyPhone_WhenAlreadyVerified_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone();

        // Act
        var result = user.VerifyPhone();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.PhoneAlreadyVerified", result.Error.Code);
    }

    [Fact]
    public void UpdateKycStatus_WithNewStatus_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var result = user.UpdateKycStatus(KycStatus.Approved);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(KycStatus.Approved, user.KycStatus);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var result = user.Deactivate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.IsActive);
        Assert.NotNull(user.DeletedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.Deactivate();

        // Act
        var result = user.Deactivate();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.AlreadyDeactivated", result.Error.Code);
    }

    #region Security Features Tests

    [Fact]
    public void IsLockedOut_WhenNoLockout_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var isLockedOut = user.IsLockedOut();

        // Assert
        Assert.False(isLockedOut);
    }

    [Fact]
    public void IsLockedOut_WhenLockoutExpired_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Simulate 5 failed login attempts to trigger lockout
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Manually set lockout to past time (simulate expiry)
        typeof(User)
            .GetProperty(nameof(User.LockoutEnd))!
            .SetValue(user, DateTime.UtcNow.AddMinutes(-1));

        // Act
        var isLockedOut = user.IsLockedOut();

        // Assert
        Assert.False(isLockedOut);
    }

    [Fact]
    public void RecordFailedLogin_WhenFirstAttempt_ShouldIncrementCounter()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var result = user.RecordFailedLogin();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void RecordFailedLogin_WhenFifthAttempt_ShouldLockAccount()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Simulate 4 failed attempts
        for (int i = 0; i < 4; i++)
        {
            user.RecordFailedLogin();
        }

        // Act
        var result = user.RecordFailedLogin();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.AccountLocked", result.Error.Code);
        Assert.Equal(5, user.FailedLoginAttempts);
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd > DateTime.UtcNow);
        Assert.True(user.IsLockedOut());
    }

    [Fact]
    public void ResetFailedLoginAttempts_ShouldClearCounterAndLockout()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Simulate failed attempts
        for (int i = 0; i < 3; i++)
        {
            user.RecordFailedLogin();
        }

        // Act
        user.ResetFailedLoginAttempts();

        // Assert
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public void SetRefreshToken_ShouldStoreTokenAndExpiry()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        var refreshToken = "test_refresh_token_xyz123";
        var expiryTime = DateTime.UtcNow.AddDays(7);

        // Act
        user.SetRefreshToken(refreshToken, expiryTime);

        // Assert
        Assert.Equal(refreshToken, user.RefreshToken);
        Assert.Equal(expiryTime, user.RefreshTokenExpiryTime);
    }

    [Fact]
    public void ValidateRefreshToken_WithValidToken_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        var refreshToken = "test_refresh_token_xyz123";
        var expiryTime = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, expiryTime);

        // Act
        var result = user.ValidateRefreshToken(refreshToken);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRefreshToken_WithNoTokenSet_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var result = user.ValidateRefreshToken("any_token");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("RefreshToken.Invalid", result.Error.Code);
    }

    [Fact]
    public void ValidateRefreshToken_WithMismatchedToken_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.SetRefreshToken("correct_token", DateTime.UtcNow.AddDays(7));

        // Act
        var result = user.ValidateRefreshToken("wrong_token");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("RefreshToken.Invalid", result.Error.Code);
    }

    [Fact]
    public void ValidateRefreshToken_WithExpiredToken_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        var refreshToken = "test_refresh_token_xyz123";
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddMinutes(-1)); // Expired

        // Act
        var result = user.ValidateRefreshToken(refreshToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("RefreshToken.Expired", result.Error.Code);
    }

    [Fact]
    public void RevokeRefreshToken_ShouldClearTokenAndExpiry()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.SetRefreshToken("token", DateTime.UtcNow.AddDays(7));

        // Act
        user.RevokeRefreshToken();

        // Assert
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
    }

    [Fact]
    public void GeneratePasswordResetToken_ShouldCreateValidToken()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var token = user.GeneratePasswordResetToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal(token, user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetTokenExpiry);
        Assert.True(user.PasswordResetTokenExpiry > DateTime.UtcNow);
    }

    [Fact]
    public void ValidatePasswordResetToken_WithValidToken_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        var token = user.GeneratePasswordResetToken();

        // Act
        var result = user.ValidatePasswordResetToken(token);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidatePasswordResetToken_WithNoTokenSet_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var result = user.ValidatePasswordResetToken("any_token");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PasswordReset.TokenInvalid", result.Error.Code);
    }

    [Fact]
    public void ValidatePasswordResetToken_WithExpiredToken_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        var token = user.GeneratePasswordResetToken();

        // Manually expire the token
        typeof(User)
            .GetProperty(nameof(User.PasswordResetTokenExpiry))!
            .SetValue(user, DateTime.UtcNow.AddMinutes(-1));

        // Act
        var result = user.ValidatePasswordResetToken(token);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PasswordReset.TokenExpired", result.Error.Code);
    }

    [Fact]
    public void ResetPassword_WithValidToken_ShouldUpdatePasswordHash()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var oldPasswordHash = "old_hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, oldPasswordHash, fullName).Value;
        var token = user.GeneratePasswordResetToken();
        var newPasswordHash = "new_hashed_password";

        // Act
        var result = user.ResetPassword(newPasswordHash);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetTokenExpiry);
    }

    [Fact]
    public void ResetPassword_WithNoTokenSet_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        var result = user.ResetPassword("new_password_hash");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PasswordReset.NoTokenSet", result.Error.Code);
    }

    [Fact]
    public void ResetPassword_WithEmptyPasswordHash_ShouldFail()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.GeneratePasswordResetToken();

        // Act
        var result = user.ResetPassword("");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.PasswordHashEmpty", result.Error.Code);
    }

    [Fact]
    public void EnableTwoFactor_ShouldSetTwoFactorEnabledToTrue()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Act
        user.EnableTwoFactor();

        // Assert
        Assert.True(user.TwoFactorEnabled);
    }

    [Fact]
    public void DisableTwoFactor_ShouldSetTwoFactorEnabledToFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.EnableTwoFactor();

        // Act
        user.DisableTwoFactor();

        // Assert
        Assert.False(user.TwoFactorEnabled);
    }

    #endregion
}
