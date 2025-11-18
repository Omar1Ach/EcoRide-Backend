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
}
