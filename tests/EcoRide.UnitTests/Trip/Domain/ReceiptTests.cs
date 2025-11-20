using EcoRide.Modules.Trip.Domain.Entities;

namespace EcoRide.UnitTests.Trip.Domain;

/// <summary>
/// Unit tests for Receipt entity
/// Tests receipt generation (US-006: TC-055)
/// </summary>
public class ReceiptTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var vehicleCode = "ECO-1234";
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = Receipt.Create(
            tripId,
            userId,
            vehicleCode,
            startTime,
            endTime,
            durationMinutes: 60,
            distanceMeters: 6000,
            startLatitude: 33.5731,
            startLongitude: -7.5898,
            endLatitude: 33.5831,
            endLongitude: -7.5998,
            baseCost: 5.0m,
            timeCost: 90.0m,
            totalCost: 95.0m,
            paymentMethod: "Wallet",
            paymentDetails: "Paid from Wallet",
            walletBalanceBefore: 150.0m,
            walletBalanceAfter: 55.0m);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(tripId, result.Value.TripId);
        Assert.Equal(userId, result.Value.UserId);
        Assert.StartsWith("RCP-", result.Value.ReceiptNumber);
        Assert.Equal(95.0m, result.Value.TotalCost);
    }

    [Fact]
    public void Create_WithEmptyTripId_ShouldFail()
    {
        // Act
        var result = Receipt.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "ECO-1234",
            DateTime.UtcNow,
            DateTime.UtcNow,
            60, 6000,
            33.5, -7.5, 33.6, -7.6,
            5.0m, 90.0m, 95.0m,
            "Wallet", "Paid",
            150.0m, 55.0m);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Receipt.InvalidTripId", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        // Act
        var result = Receipt.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "ECO-1234",
            DateTime.UtcNow,
            DateTime.UtcNow,
            60, 6000,
            33.5, -7.5, 33.6, -7.6,
            5.0m, 90.0m, 95.0m,
            "Wallet", "Paid",
            150.0m, 55.0m);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Receipt.InvalidUserId", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyVehicleCode_ShouldFail()
    {
        // Act
        var result = Receipt.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            DateTime.UtcNow,
            DateTime.UtcNow,
            60, 6000,
            33.5, -7.5, 33.6, -7.6,
            5.0m, 90.0m, 95.0m,
            "Wallet", "Paid",
            150.0m, 55.0m);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Receipt.InvalidVehicleCode", result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeTotalCost_ShouldFail()
    {
        // Act
        var result = Receipt.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ECO-1234",
            DateTime.UtcNow,
            DateTime.UtcNow,
            60, 6000,
            33.5, -7.5, 33.6, -7.6,
            5.0m, 90.0m, -10.0m,
            "Wallet", "Paid",
            150.0m, 55.0m);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Receipt.InvalidTotalCost", result.Error.Code);
    }

    [Fact]
    public void Create_GeneratesUniqueReceiptNumbers()
    {
        // Act
        var receipt1 = Receipt.Create(
            Guid.NewGuid(), Guid.NewGuid(), "ECO-1234",
            DateTime.UtcNow, DateTime.UtcNow,
            60, 6000, 33.5, -7.5, 33.6, -7.6,
            5.0m, 90.0m, 95.0m,
            "Wallet", "Paid", 150.0m, 55.0m).Value;

        var receipt2 = Receipt.Create(
            Guid.NewGuid(), Guid.NewGuid(), "ECO-5678",
            DateTime.UtcNow, DateTime.UtcNow,
            30, 3000, 33.5, -7.5, 33.6, -7.6,
            5.0m, 45.0m, 50.0m,
            "CreditCard", "Paid with Visa ****1234", 150.0m, 150.0m).Value;

        // Assert
        Assert.NotEqual(receipt1.ReceiptNumber, receipt2.ReceiptNumber);
        Assert.Matches(@"^RCP-\d{8}-[A-F0-9]{6}$", receipt1.ReceiptNumber);
        Assert.Matches(@"^RCP-\d{8}-[A-F0-9]{6}$", receipt2.ReceiptNumber);
    }

    [Fact]
    public void ToString_ShouldReturnReceiptNumber()
    {
        // Arrange
        var receipt = Receipt.Create(
            Guid.NewGuid(), Guid.NewGuid(), "ECO-1234",
            DateTime.UtcNow, DateTime.UtcNow,
            60, 6000, 33.5, -7.5, 33.6, -7.6,
            5.0m, 90.0m, 95.0m,
            "Wallet", "Paid", 150.0m, 55.0m).Value;

        // Act
        var str = receipt.ToString();

        // Assert
        Assert.Equal(receipt.ReceiptNumber, str);
    }
}
