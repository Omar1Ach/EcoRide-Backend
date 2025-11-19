using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Application.Commands.Login;

/// <summary>
/// Handler for LoginCommand with account lockout and 2FA support
/// </summary>
public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ISmsService _smsService;
    private readonly IOtpRepository _otpRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ISmsService smsService,
        IOtpRepository otpRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _smsService = smsService;
        _otpRepository = otpRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Validate email
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(emailResult.Error);
        }

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            return Result.Failure<LoginResponse>(
                new Error("Login.InvalidCredentials", "Invalid email or password"));
        }

        // Check if account is locked
        if (user.IsLockedOut())
        {
            return Result.Failure<LoginResponse>(
                new Error("Login.AccountLocked",
                    $"Account is locked due to multiple failed login attempts. Try again after {user.LockoutEnd:yyyy-MM-dd HH:mm:ss} UTC"));
        }

        // Check if account is active
        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>(
                new Error("Login.AccountInactive", "Account is inactive. Please contact support"));
        }

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Record failed login attempt
            var failedResult = user.RecordFailedLogin();
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (failedResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(failedResult.Error);
            }

            return Result.Failure<LoginResponse>(
                new Error("Login.InvalidCredentials", "Invalid email or password"));
        }

        // Check if 2FA is enabled and user requested 2FA login
        if (user.TwoFactorEnabled || request.Enable2FA)
        {
            // Generate and send OTP for 2FA
            var otpCode = Domain.Aggregates.OtpCode.Generate(user.PhoneNumber);

            var smsResult = await _smsService.SendOtpAsync(
                user.PhoneNumber,
                otpCode.Code,
                cancellationToken);

            if (smsResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(smsResult.Error);
            }

            await _otpRepository.AddAsync(otpCode, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new LoginResponse(
                user.Id,
                user.Email,
                null, // No access token yet - needs 2FA verification
                null, // No refresh token yet
                null, // No expiry
                true, // Requires 2FA
                "2FA code sent to your phone. Please verify to complete login"));
        }

        // Successful login - reset failed attempts
        user.ResetFailedLoginAttempts();

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            user.Role.ToString());

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // 7 days validity

        user.SetRefreshToken(refreshToken, refreshTokenExpiry);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse(
            user.Id,
            user.Email,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15), // Access token expires in 15 minutes
            false, // No 2FA required
            "Login successful"));
    }
}
