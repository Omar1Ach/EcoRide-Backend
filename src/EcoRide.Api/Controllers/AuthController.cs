using EcoRide.Api.Models.Auth;
using EcoRide.Modules.Security.Application.Commands.Login;
using EcoRide.Modules.Security.Application.Commands.RegisterUser;
using EcoRide.Modules.Security.Application.Commands.ResendOtp;
using EcoRide.Modules.Security.Application.Commands.VerifyOtp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRide.Api.Controllers;

/// <summary>
/// Authentication and registration endpoints
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user and send OTP for verification
    /// </summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUp([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.PhoneNumber,
            request.Password,
            request.FullName);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Message
            });
        }

        return StatusCode(201, new
        {
            userId = result.Value.UserId,
            message = result.Value.Message
        });
    }

    /// <summary>
    /// Verify OTP code and complete registration
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var command = new VerifyOtpCommand(
            request.PhoneNumber,
            request.Code);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(new
        {
            userId = result.Value.UserId,
            email = result.Value.Email,
            accessToken = result.Value.AccessToken,
            refreshToken = result.Value.RefreshToken,
            expiresAt = result.Value.ExpiresAt
        });
    }

    /// <summary>
    /// Resend OTP code to the user's phone number
    /// </summary>
    [HttpPost("resend-otp")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
    {
        var command = new ResendOtpCommand(request.PhoneNumber);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            // Return 429 for rate limit errors
            if (result.Error.Code == "Otp.RateLimitExceeded")
            {
                return StatusCode(429, new
                {
                    error = result.Error.Code,
                    message = result.Error.Message
                });
            }

            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(new
        {
            message = result.Value
        });
    }

    /// <summary>
    /// Login with email and password
    /// Supports account lockout after 5 failed attempts and optional 2FA
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.Enable2FA);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            // Return 401 for invalid credentials
            if (result.Error.Code == "Login.InvalidCredentials" ||
                result.Error.Code == "Login.AccountLocked" ||
                result.Error.Code == "Login.AccountInactive")
            {
                return Unauthorized(new
                {
                    error = result.Error.Code,
                    message = result.Error.Message
                });
            }

            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(result.Value);
    }
}
