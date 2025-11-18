using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.UnitTests.Security.Domain;

public class OtpCodeTests
{
    [Fact]
    public void Generate_ShouldCreateValidOtpCode()
    {
        // Arrange
        var phone = PhoneNumber.Create("+212612345678").Value;

        // Act
        var otpCode = OtpCode.Generate(phone);

        // Assert
        Assert.NotNull(otpCode);
        Assert.Equal(phone, otpCode.PhoneNumber);
        Assert.Equal(6, otpCode.Code.Length);
        Assert.True(int.TryParse(otpCode.Code, out _));
        Assert.Equal(0, otpCode.Attempts);
        Assert.False(otpCode.Verified);
        Assert.True(otpCode.ExpiresAt > DateTime.UtcNow);
        Assert.True(otpCode.ExpiresAt <= DateTime.UtcNow.AddMinutes(5));
        Assert.Single(otpCode.DomainEvents);
    }

    [Fact]
    public void Verify_WithCorrectCode_ShouldSucceed()
    {
        // Arrange
        var phone = PhoneNumber.Create("+212612345678").Value;
        var otpCode = OtpCode.Generate(phone);
        var correctCode = otpCode.Code;

        // Act
        var result = otpCode.Verify(correctCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.True(otpCode.Verified);
        Assert.Equal(1, otpCode.Attempts);
    }

    [Fact]
    public void Verify_WithIncorrectCode_ShouldFail()
    {
        // Arrange
        var phone = PhoneNumber.Create("+212612345678").Value;
        var otpCode = OtpCode.Generate(phone);
        var incorrectCode = "000000";

        // Act
        var result = otpCode.Verify(incorrectCode);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Otp.InvalidCode", result.Error.Code);
        Assert.False(otpCode.Verified);
        Assert.Equal(1, otpCode.Attempts);
    }

    [Fact]
    public void Verify_AfterMaxAttempts_ShouldFail()
    {
        // Arrange
        var phone = PhoneNumber.Create("+212612345678").Value;
        var otpCode = OtpCode.Generate(phone);

        // Act - Make 3 failed attempts
        otpCode.Verify("000000");
        otpCode.Verify("111111");
        otpCode.Verify("222222");

        // Try 4th attempt
        var result = otpCode.Verify("333333");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Otp.MaxAttemptsExceeded", result.Error.Code);
        Assert.Equal(3, otpCode.Attempts); // Should not increment beyond max
    }

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var phone = PhoneNumber.Create("+212612345678").Value;
        var otpCode = OtpCode.Generate(phone);

        // Act
        var isExpired = otpCode.IsExpired();

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void CanRetry_WhenUnderMaxAttempts_ShouldReturnTrue()
    {
        // Arrange
        var phone = PhoneNumber.Create("+212612345678").Value;
        var otpCode = OtpCode.Generate(phone);
        otpCode.Verify("000000"); // 1 attempt

        // Act
        var canRetry = otpCode.CanRetry();

        // Assert
        Assert.True(canRetry);
    }

    [Fact]
    public void CanRetry_AtMaxAttempts_ShouldReturnFalse()
    {
        // Arrange
        var phone = PhoneNumber.Create("+212612345678").Value;
        var otpCode = OtpCode.Generate(phone);
        otpCode.Verify("000000"); // 1 attempt
        otpCode.Verify("111111"); // 2 attempts
        otpCode.Verify("222222"); // 3 attempts

        // Act
        var canRetry = otpCode.CanRetry();

        // Assert
        Assert.False(canRetry);
    }
}
