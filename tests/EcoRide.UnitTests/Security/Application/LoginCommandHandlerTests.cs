using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Commands.Login;
using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Enums;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;
using Moq;

namespace EcoRide.UnitTests.Security.Application;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISecurityUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<ISecurityUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _smsServiceMock = new Mock<ISmsService>();
        _otpRepositoryMock = new Mock<IOtpRepository>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _smsServiceMock.Object,
            _otpRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessWithTokens()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone(); // Make user verified and active

        var command = new LoginCommand("test@example.com", "plain_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify("plain_password", passwordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, email.Value, "User"))
            .Returns("access_token_xyz");

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token_xyz");

        _userRepositoryMock
            .Setup(x => x.Update(user));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access_token_xyz", result.Value.AccessToken);
        Assert.Equal("refresh_token_xyz", result.Value.RefreshToken);
        Assert.False(result.Value.Requires2FA);
        Assert.Equal("Login successful", result.Value.Message);

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand("invalid_email", "password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Email.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnInvalidCredentialsError()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Login.InvalidCredentials", result.Error.Code);
        Assert.Equal("Invalid email or password", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithLockedOutAccount_ShouldReturnAccountLockedError()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;

        // Lock the account by recording 5 failed login attempts
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        var command = new LoginCommand("test@example.com", "password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Login.AccountLocked", result.Error.Code);
        Assert.Contains("locked", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_ShouldReturnAccountInactiveError()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.Deactivate(); // Deactivate the account

        var command = new LoginCommand("test@example.com", "password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Login.AccountInactive", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldRecordFailedLoginAndReturnError()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone();

        var command = new LoginCommand("test@example.com", "wrong_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify("wrong_password", passwordHash))
            .Returns(false);

        _userRepositoryMock
            .Setup(x => x.Update(user));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Login.InvalidCredentials", result.Error.Code);
        Assert.Equal(1, user.FailedLoginAttempts);

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_OnFifthAttempt_ShouldLockAccount()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone();

        // Simulate 4 previous failed attempts
        for (int i = 0; i < 4; i++)
        {
            user.RecordFailedLogin();
        }

        var command = new LoginCommand("test@example.com", "wrong_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify("wrong_password", passwordHash))
            .Returns(false);

        _userRepositoryMock
            .Setup(x => x.Update(user));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.AccountLocked", result.Error.Code);
        Assert.Equal(5, user.FailedLoginAttempts);
        Assert.True(user.IsLockedOut());
    }

    [Fact]
    public async Task Handle_WithTwoFactorEnabled_ShouldSendOtpAndReturnRequires2FA()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone();
        user.EnableTwoFactor(); // Enable 2FA

        var command = new LoginCommand("test@example.com", "plain_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify("plain_password", passwordHash))
            .Returns(true);

        _smsServiceMock
            .Setup(x => x.SendOtpAsync(phone.Value, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _otpRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<OtpCode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Requires2FA);
        Assert.Null(result.Value.AccessToken);
        Assert.Null(result.Value.RefreshToken);
        Assert.Contains("2FA", result.Value.Message);

        _smsServiceMock.Verify(
            x => x.SendOtpAsync(phone.Value, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _otpRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<OtpCode>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEnable2FAParameter_ShouldSendOtpEvenIfNotEnabledOnAccount()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone();
        // Note: TwoFactorEnabled is false, but command has Enable2FA = true

        var command = new LoginCommand("test@example.com", "plain_password", Enable2FA: true);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify("plain_password", passwordHash))
            .Returns(true);

        _smsServiceMock
            .Setup(x => x.SendOtpAsync(phone.Value, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _otpRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<OtpCode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Requires2FA);
        Assert.Null(result.Value.AccessToken);
        Assert.Null(result.Value.RefreshToken);

        _smsServiceMock.Verify(
            x => x.SendOtpAsync(phone.Value, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithSuccessfulLogin_ShouldResetFailedLoginAttempts()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone();

        // Simulate previous failed attempts
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        var command = new LoginCommand("test@example.com", "plain_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify("plain_password", passwordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, email.Value, "User"))
            .Returns("access_token");

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public async Task Handle_WithSuccessfulLogin_ShouldSetRefreshToken()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+212612345678").Value;
        var passwordHash = "hashed_password";
        var fullName = FullName.Create("John Doe").Value;
        var user = User.CreatePendingRegistration(email, phone, passwordHash, fullName).Value;
        user.VerifyPhone();

        var command = new LoginCommand("test@example.com", "plain_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify("plain_password", passwordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(user.Id, email.Value, "User"))
            .Returns("access_token");

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token_xyz123");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("refresh_token_xyz123", user.RefreshToken);
        Assert.NotNull(user.RefreshTokenExpiryTime);
        Assert.True(user.RefreshTokenExpiryTime > DateTime.UtcNow);
    }
}
