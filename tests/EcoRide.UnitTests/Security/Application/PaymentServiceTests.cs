using EcoRide.BuildingBlocks.Application.Services;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Entities;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentMethod = EcoRide.BuildingBlocks.Application.Services.PaymentMethod;

namespace EcoRide.UnitTests.Security.Application;

/// <summary>
/// Unit tests for PaymentService
/// Tests payment processing with wallet/credit card fallback and retry logic (US-006: TC-054)
/// </summary>
public class PaymentServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPaymentMethodRepository> _paymentMethodRepositoryMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _paymentMethodRepositoryMock = new Mock<IPaymentMethodRepository>();
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _paymentService = new PaymentService(
            _userRepositoryMock.Object,
            _paymentMethodRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessTripPaymentAsync_WithSufficientWalletBalance_ShouldUseWallet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 50.0m;

        var user = User.CreatePendingRegistration(
            Email.Create("test@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(100m); // User has 100 MAD in wallet

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentMethod.Wallet, result.Value.Method);
        Assert.Equal("Paid from Wallet", result.Value.Message);
        Assert.Equal(50.0m, user.WalletBalance); // 100 - 50 = 50
        _paymentMethodRepositoryMock.Verify(
            x => x.GetDefaultByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessTripPaymentAsync_WithInsufficientWalletBalance_ShouldFallbackToCreditCard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 150.0m;

        var user = User.CreatePendingRegistration(
            Email.Create("test@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(50m); // User has only 50 MAD in wallet (insufficient)

        var paymentMethod = PaymentMethodEntity.Create(
            userId,
            "4242",
            "Visa",
            expiryMonth: 12,
            expiryYear: 2030).Value;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _paymentMethodRepositoryMock
            .Setup(x => x.GetDefaultByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethod);

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentMethod.CreditCard, result.Value.Method);
        Assert.Equal("Paid with Visa ****4242", result.Value.Message);
        Assert.Equal(50m, user.WalletBalance); // Wallet balance unchanged
        Assert.Equal("4242", result.Value.CardLast4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-0.01)]
    public async Task ProcessTripPaymentAsync_WithInvalidAmount_ShouldFail(decimal amount)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Payment.InvalidAmount", result.Error.Code);
        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessTripPaymentAsync_WithNonExistentUser_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 50.0m;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Payment.UserNotFound", result.Error.Code);
    }

    [Fact]
    public async Task ProcessTripPaymentAsync_WithInsufficientWalletAndNoPaymentMethod_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 150.0m;

        var user = User.CreatePendingRegistration(
            Email.Create("test@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(50m); // Insufficient wallet balance

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _paymentMethodRepositoryMock
            .Setup(x => x.GetDefaultByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentMethodEntity?)null); // No payment method

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Payment.NoPaymentMethod", result.Error.Code);
    }

    [Fact]
    public async Task ProcessTripPaymentAsync_WithExactWalletBalance_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 100.0m;

        var user = User.CreatePendingRegistration(
            Email.Create("test@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(100m); // Exact amount

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentMethod.Wallet, result.Value.Method);
        Assert.Equal(0m, user.WalletBalance); // Wallet depleted to 0
    }

    [Fact]
    public async Task ProcessTripPaymentAsync_WithSmallAmount_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 0.01m; // Minimum amount

        var user = User.CreatePendingRegistration(
            Email.Create("test@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(1m);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentMethod.Wallet, result.Value.Method);
        Assert.Equal(0.99m, user.WalletBalance);
    }

    [Fact]
    public async Task ProcessTripPaymentAsync_WalletPaymentPreferredOverCreditCard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 50.0m;

        var user = User.CreatePendingRegistration(
            Email.Create("test@example.com").Value,
            PhoneNumber.Create("+212600000001").Value,
            "hashedPassword",
            FullName.Create("Test User").Value).Value;

        user.AddToWallet(100m); // Sufficient wallet balance

        // User also has a credit card, but wallet should be preferred
        var paymentMethod = PaymentMethodEntity.Create(
            userId,
            "4242",
            "Visa",
            expiryMonth: 12,
            expiryYear: 2030).Value;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _paymentMethodRepositoryMock
            .Setup(x => x.GetDefaultByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethod);

        // Act
        var result = await _paymentService.ProcessTripPaymentAsync(userId, amount, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentMethod.Wallet, result.Value.Method);
        // Verify credit card was never queried
        _paymentMethodRepositoryMock.Verify(
            x => x.GetDefaultByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
