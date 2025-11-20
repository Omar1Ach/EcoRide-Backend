using EcoRide.Modules.Trip.Application.Commands.EndTrip;
using EcoRide.Modules.Trip.Application.Commands.RateTrip;
using EcoRide.Modules.Trip.Application.Commands.StartTrip;
using EcoRide.Modules.Trip.Application.DTOs;
using EcoRide.Modules.Trip.Application.Queries.GetActiveTripStats;
using EcoRide.Modules.Trip.Application.Queries.GetTripById;
using EcoRide.Modules.Trip.Application.Queries.GetTripHistory;
using EcoRide.Modules.Trip.Application.Queries.GetTripReceipt;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRide.Api.Controllers;

/// <summary>
/// API endpoints for trip management
/// Implements US-004: Start Trip (QR Scan)
/// Implements US-005: Active Trip Tracking
/// Implements US-006: End Trip & Payment
/// Implements US-007: Trip History
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

    /// <summary>
    /// Get user's trip history with pagination (TC-060 to TC-064)
    /// US-007: Trip History - View past trips sorted by date
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetTripHistory(
        [FromQuery] Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTripHistoryQuery(userId, pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get detailed trip information by ID
    /// US-007: Trip History - View trip details
    /// </summary>
    [HttpGet("{tripId}")]
    public async Task<IActionResult> GetTripById(
        [FromRoute] Guid tripId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetTripByIdQuery(tripId, userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Trip.NotFound"
                ? NotFound(new { error = result.Error.Message, code = result.Error.Code })
                : BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get trip receipt for viewing/downloading
    /// US-007: Trip History - View receipt
    /// </summary>
    [HttpGet("{tripId}/receipt")]
    public async Task<IActionResult> GetTripReceipt(
        [FromRoute] Guid tripId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetTripReceiptQuery(tripId, userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Trip.NotFound" || result.Error.Code == "Receipt.NotFound"
                ? NotFound(new { error = result.Error.Message, code = result.Error.Code })
                : BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// End the active trip and process payment (TC-050 to TC-052)
    /// US-006: End Trip & Payment
    /// </summary>
    [HttpPost("end")]
    public async Task<IActionResult> EndTrip(
        [FromBody] EndTripRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EndTripCommand(
            request.UserId,
            request.EndLatitude,
            request.EndLongitude);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Rate a completed trip (US-006: Trip rating feature)
    /// </summary>
    [HttpPost("{tripId}/rate")]
    public async Task<IActionResult> RateTrip(
        [FromRoute] Guid tripId,
        [FromBody] RateTripRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RateTripCommand(
            tripId,
            request.UserId,
            request.Stars,
            request.Comment);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(new { message = "Trip rated successfully" });
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

/// <summary>
/// Request model for ending a trip
/// </summary>
public sealed record EndTripRequest(
    Guid UserId,
    double EndLatitude,
    double EndLongitude);

/// <summary>
/// Request model for rating a trip
/// </summary>
public sealed record RateTripRequest(
    Guid UserId,
    int Stars,
    string? Comment = null);
