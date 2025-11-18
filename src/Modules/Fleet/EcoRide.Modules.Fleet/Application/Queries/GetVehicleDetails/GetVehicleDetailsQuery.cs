using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Fleet.Application.DTOs;

namespace EcoRide.Modules.Fleet.Application.Queries.GetVehicleDetails;

/// <summary>
/// Query to get detailed information about a specific vehicle
/// </summary>
public sealed record GetVehicleDetailsQuery(
    Guid VehicleId,
    double? UserLatitude = null,
    double? UserLongitude = null
) : IQuery<VehicleDto>;
