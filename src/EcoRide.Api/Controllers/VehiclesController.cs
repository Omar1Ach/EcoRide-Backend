using EcoRide.Modules.Fleet.Application.Queries.GetNearbyVehicles;
using EcoRide.Modules.Fleet.Application.Queries.GetVehicleDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRide.Api.Controllers;

/// <summary>
/// Vehicle discovery and management endpoints
/// </summary>
[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get nearby available vehicles within specified radius
    /// </summary>
    /// <param name="latitude">User's latitude</param>
    /// <param name="longitude">User's longitude</param>
    /// <param name="radiusMeters">Search radius in meters (default: 500m)</param>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearbyVehicles(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] int radiusMeters = 500)
    {
        var query = new GetNearbyVehiclesQuery(latitude, longitude, radiusMeters);

        var result = await _mediator.Send(query);

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
            vehicles = result.Value,
            count = result.Value.Count
        });
    }

    /// <summary>
    /// Get detailed information about a specific vehicle
    /// </summary>
    /// <param name="id">Vehicle ID</param>
    /// <param name="userLatitude">User's latitude (optional, for distance calculation)</param>
    /// <param name="userLongitude">User's longitude (optional, for distance calculation)</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicleDetails(
        [FromRoute] Guid id,
        [FromQuery] double? userLatitude = null,
        [FromQuery] double? userLongitude = null)
    {
        var query = new GetVehicleDetailsQuery(id, userLatitude, userLongitude);

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Vehicle.NotFound")
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
