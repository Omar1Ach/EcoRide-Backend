using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Application.Commands.VerifyOtp;

/// <summary>
/// Handler for VerifyOtpCommand
/// </summary>
public sealed class VerifyOtpCommandHandler : ICommandHandler<VerifyOtpCommand, VerifyOtpResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyOtpCommandHandler(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VerifyOtpResponse>> Handle(
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        // Create phone value object
        var phoneResult = PhoneNumber.Create(request.PhoneNumber);
        if (phoneResult.IsFailure)
        {
            return Result.Failure<VerifyOtpResponse>(phoneResult.Error);
        }

        // Get latest OTP for phone
        var otpCode = await _otpRepository.GetLatestByPhoneAsync(
            phoneResult.Value,
            cancellationToken);

        if (otpCode is null)
        {
            return Result.Failure<VerifyOtpResponse>(
                new Error("Otp.NotFound", "No active OTP found for this phone number"));
        }

        // Verify OTP
        var verifyResult = otpCode.Verify(request.Code);
        if (verifyResult.IsFailure)
        {
            _otpRepository.Update(otpCode);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<VerifyOtpResponse>(verifyResult.Error);
        }

        // Get user by phone
        var user = await _userRepository.GetByPhoneAsync(
            phoneResult.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<VerifyOtpResponse>(
                new Error("User.NotFound", "User not found"));
        }

        // Mark phone as verified
        var verifyPhoneResult = user.VerifyPhone();
        if (verifyPhoneResult.IsFailure)
        {
            return Result.Failure<VerifyOtpResponse>(verifyPhoneResult.Error);
        }

        // Generate JWT tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            user.Role.ToString());

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Update repositories
        _userRepository.Update(user);
        _otpRepository.Update(otpCode);

        // Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new VerifyOtpResponse(
            user.Id,
            user.Email,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15))); // Access token expires in 15 minutes
    }
}
