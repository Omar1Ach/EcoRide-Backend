using EcoRide.Modules.Trip.Application.Commands.CancelReservation;
using EcoRide.Modules.Trip.Application.Commands.CreateReservation;
using EcoRide.Modules.Trip.Application.Queries.GetActiveReservation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRide.Api.Controllers;

/// <summary>
/// API endpoints for vehicle reservations
/// Implements US-003: Vehicle Reservation
/// </summary>
[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ISender _sender;

    public ReservationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Create a new reservation
    /// POST /api/reservations
    /// </summary>
    /// <param name="request">User ID and Vehicle ID</param>
    /// <returns>Created reservation with countdown timer</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(request.UserId, request.VehicleId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message });
        }

        return CreatedAtAction(
            nameof(GetActiveReservation),
            new { userId = request.UserId },
            result.Value);
    }

    /// <summary>
    /// Get user's active reservation
    /// GET /api/reservations/active?userId={userId}
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Active reservation or null</returns>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveReservation(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetActiveReservationQuery(userId);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message });
        }

        if (result.Value is null)
        {
            return NotFound(new { message = "No active reservation found" });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Cancel a reservation
    /// DELETE /api/reservations/{id}
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="request">User ID for authorization</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(
        Guid id,
        [FromBody] CancelReservationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelReservationCommand(id, request.UserId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Reservation.NotFound")
            {
                return NotFound(new { error = result.Error.Message });
            }

            return BadRequest(new { error = result.Error.Message });
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for creating a reservation
/// </summary>
public record CreateReservationRequest(Guid UserId, Guid VehicleId);

/// <summary>
/// Request model for cancelling a reservation
/// </summary>
public record CancelReservationRequest(Guid UserId);
