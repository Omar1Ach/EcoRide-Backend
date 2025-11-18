using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Application.Commands.ResendOtp;

/// <summary>
/// Handler for ResendOtpCommand
/// </summary>
public sealed class ResendOtpCommandHandler : ICommandHandler<ResendOtpCommand, string>
{
    private readonly IOtpRepository _otpRepository;
    private readonly ISmsService _smsService;
    private readonly IUnitOfWork _unitOfWork;

    public ResendOtpCommandHandler(
        IOtpRepository otpRepository,
        ISmsService smsService,
        IUnitOfWork unitOfWork)
    {
        _otpRepository = otpRepository;
        _smsService = smsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(
        ResendOtpCommand request,
        CancellationToken cancellationToken)
    {
        // Create phone value object
        var phoneResult = PhoneNumber.Create(request.PhoneNumber);
        if (phoneResult.IsFailure)
        {
            return Result.Failure<string>(phoneResult.Error);
        }

        // Check rate limiting: max 3 requests per minute
        var recentCount = await _otpRepository.CountRecentOtpRequestsAsync(
            phoneResult.Value,
            TimeSpan.FromMinutes(1),
            cancellationToken);

        if (recentCount >= 3)
        {
            return Result.Failure<string>(
                new Error("Otp.RateLimitExceeded", "Too many OTP requests. Please wait before requesting again."));
        }

        // Generate new OTP
        var otpCode = OtpCode.Generate(phoneResult.Value);

        // Send SMS
        var smsResult = await _smsService.SendOtpAsync(
            phoneResult.Value,
            otpCode.Code,
            cancellationToken);

        if (smsResult.IsFailure)
        {
            return Result.Failure<string>(smsResult.Error);
        }

        // Save to repository
        await _otpRepository.AddAsync(otpCode, cancellationToken);

        // Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("OTP resent successfully");
    }
}
