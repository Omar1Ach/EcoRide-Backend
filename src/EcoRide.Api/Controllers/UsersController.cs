using EcoRide.Modules.Security.Application.Commands.UpdateUserProfile;
using EcoRide.Modules.Security.Application.Commands.UpdateUserSettings;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Application.Queries.GetUserProfile;
using EcoRide.Modules.Security.Application.Queries.GetUserSettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRide.Api.Controllers;

/// <summary>
/// User profile and settings management endpoints
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user profile information
    /// </summary>
    /// <param name="userId">The user ID</param>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile([FromQuery] Guid userId)
    {
        var query = new GetUserProfileQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new
            {
                error = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The profile update request</param>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        [FromQuery] Guid userId,
        [FromBody] UpdateProfileRequest request)
    {
        var command = new UpdateUserProfileCommand(
            userId,
            request.FullName,
            request.Email,
            request.PhoneNumber);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            // Return 404 for user not found
            if (result.Error.Code == "User.NotFound")
            {
                return NotFound(new
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

    /// <summary>
    /// Get user settings
    /// </summary>
    /// <param name="userId">The user ID</param>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettings([FromQuery] Guid userId)
    {
        var query = new GetUserSettingsQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new
            {
                error = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update user settings
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The settings update request</param>
    [HttpPut("settings")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSettings(
        [FromQuery] Guid userId,
        [FromBody] UpdateSettingsRequest request)
    {
        var command = new UpdateUserSettingsCommand(
            userId,
            request.PushNotificationsEnabled,
            request.DarkModeEnabled,
            request.HapticFeedbackEnabled,
            request.LanguageCode);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            // Return 404 for user not found
            if (result.Error.Code == "User.NotFound")
            {
                return NotFound(new
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
