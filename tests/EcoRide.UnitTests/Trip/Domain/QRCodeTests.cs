using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.UnitTests.Trip.Domain;

/// <summary>
/// Unit tests for QRCode value object
/// Tests QR code format validation (ECO-XXXX)
/// </summary>
public class QRCodeTests
{
    [Theory]
    [InlineData("ECO-1234")]
    [InlineData("ECO-0001")]
    [InlineData("ECO-9999")]
    [InlineData("eco-1234")] // Should normalize to uppercase
    [InlineData(" ECO-5678 ")] // Should trim whitespace
    public void Create_WithValidFormat_ShouldSucceed(string code)
    {
        // Act
        var result = QRCode.Create(code);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.StartsWith("ECO-", result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCode_ShouldFail(string code)
    {
        // Act
        var result = QRCode.Create(code);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("QRCode.Empty", result.Error.Code);
    }

    [Theory]
    [InlineData("ECO-123")] // Too short
    [InlineData("ECO-12345")] // Too long
    [InlineData("ECO-ABCD")] // Not digits
    [InlineData("BIKE-1234")] // Wrong prefix
    [InlineData("1234")] // No prefix
    [InlineData("ECO1234")] // Missing dash
    public void Create_WithInvalidFormat_ShouldFail(string code)
    {
        // Act
        var result = QRCode.Create(code);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("QRCode.InvalidFormat", result.Error.Code);
        Assert.Contains("ECO-XXXX", result.Error.Message);
    }

    [Fact]
    public void Create_ShouldNormalizeToUpperCase()
    {
        // Act
        var result = QRCode.Create("eco-1234");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ECO-1234", result.Value.Value);
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Act
        var result = QRCode.Create("  ECO-5678  ");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ECO-5678", result.Value.Value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var qrCode = QRCode.Create("ECO-1234").Value;

        // Act
        var str = qrCode.ToString();

        // Assert
        Assert.Equal("ECO-1234", str);
    }
}
