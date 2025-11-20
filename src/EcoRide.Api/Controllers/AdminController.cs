using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Trip.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Api.Controllers;

/// <summary>
/// API endpoints for admin dashboard
/// Implements US-009: Admin Dashboard - Monitor platform operations
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IActiveTripRepository _tripRepository;
    private readonly IWalletTransactionRepository _transactionRepository;

    public AdminController(
        IUserRepository userRepository,
        IVehicleRepository vehicleRepository,
        IActiveTripRepository tripRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _userRepository = userRepository;
        _vehicleRepository = vehicleRepository;
        _tripRepository = tripRepository;
        _transactionRepository = transactionRepository;
    }

    /// <summary>
    /// Get dashboard statistics
    /// US-009: Platform monitoring
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        // Note: In production, add authentication/authorization for admin users only

        var (trips, _) = await _tripRepository.GetTripHistoryAsync(
            Guid.Empty, // Get all trips
            1,
            1000,
            cancellationToken);

        var completedTrips = trips.Count(t => t.Status == EcoRide.Modules.Trip.Domain.Enums.TripStatus.Completed);
        var activeTrips = trips.Count(t => t.Status == EcoRide.Modules.Trip.Domain.Enums.TripStatus.Active);
        var totalRevenue = trips.Where(t => t.Status == EcoRide.Modules.Trip.Domain.Enums.TripStatus.Completed)
            .Sum(t => t.TotalCost);

        var stats = new
        {
            TotalUsers = 0, // Would need to implement count query
            TotalVehicles = 0, // Would need to implement count query
            ActiveTrips = activeTrips,
            CompletedTrips = completedTrips,
            TotalRevenue = totalRevenue,
            LastUpdated = DateTime.UtcNow
        };

        return Ok(stats);
    }
}
