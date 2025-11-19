using EcoRide.Modules.Trip.Application.Commands.StartTrip;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Application.Queries.GetActiveTripStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRide.Api.Controllers;

/// <summary>
/// API endpoints for trip management
/// Implements US-004: Start Trip (QR Scan)
/// Implements US-005: Active Trip Tracking
/// </summary>
[ApiController]
[Route("api/trips")]
public class TripsController : ControllerBase
{
    private readonly ISender _sender;

    public TripsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Start a trip by scanning QR code (TC-030 to TC-034)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartTrip(
        [FromBody] StartTripRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StartTripCommand(
            request.UserId,
            request.QRCode,
            request.StartLatitude,
            request.StartLongitude);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get active trip statistics with live updates (TC-040 to TC-044)
    /// US-005: Real-time timer, cost, distance, battery
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveTripStats(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetActiveTripStatsQuery(userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get emergency contact numbers (TC-043)
    /// </summary>
    [HttpGet("emergency-contacts")]
    public IActionResult GetEmergencyContacts()
    {
        var contacts = EmergencyContacts.GetContacts();
        return Ok(contacts);
    }
}

/// <summary>
/// Request model for starting a trip
/// </summary>
public sealed record StartTripRequest(
    Guid UserId,
    string QRCode,
    double StartLatitude,
    double StartLongitude);
