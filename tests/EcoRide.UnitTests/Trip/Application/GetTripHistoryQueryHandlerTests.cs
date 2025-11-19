using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;
using EcoRide.Modules.Trip.Application.Queries.GetTripHistory;
using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Repositories;
using EcoRide.Modules.Trip.Domain.ValueObjects;
using Moq;
using Xunit;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Unit tests for GetTripHistoryQueryHandler
/// Test Scenarios: TC-060 to TC-064
/// </summary>
public class GetTripHistoryQueryHandlerTests
{
    private readonly Mock<IActiveTripRepository> _tripRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly GetTripHistoryQueryHandler _handler;

    public GetTripHistoryQueryHandlerTests()
    {
        _tripRepositoryMock = new Mock<IActiveTripRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _handler = new GetTripHistoryQueryHandler(
            _tripRepositoryMock.Object,
            _vehicleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithPageNumberLessThanOne_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetTripHistoryQuery(userId, PageNumber: 0, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Pagination.InvalidPageNumber", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithPageSizeLessThanOne_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Pagination.InvalidPageSize", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithPageSizeGreaterThan100_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 101);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Pagination.InvalidPageSize", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithNoTrips_ShouldReturnEmptyList()
    {
        // TC-061: User has 0 trips - empty state shown
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 20);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ActiveTrip>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Trips);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task Handle_With5Trips_ShouldReturnAllTrips()
    {
        // TC-060: User has 5 trips - all displayed
        // Arrange
        var userId = Guid.NewGuid();
        var trips = CreateTestTrips(userId, 5);
        var vehicles = CreateTestVehicles(5);
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 20);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((trips, 5));

        for (int i = 0; i < 5; i++)
        {
            var vehicle = vehicles[i];
            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(trips[i].VehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);
        }

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Trips.Count);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(1, result.Value.TotalPages);

        // Verify each trip has vehicle information
        foreach (var tripDto in result.Value.Trips)
        {
            Assert.NotNull(tripDto.VehicleType);
            Assert.NotNull(tripDto.VehicleCode);
            Assert.True(tripDto.DurationMinutes > 0);
            Assert.True(tripDto.DistanceMeters > 0);
            Assert.True(tripDto.CostMAD > 0);
        }
    }

    [Fact]
    public async Task Handle_With50Trips_ShouldPaginateCorrectly()
    {
        // TC-062: User has 50 trips - pagination works
        // Arrange
        var userId = Guid.NewGuid();
        var allTrips = CreateTestTrips(userId, 50);
        var firstPageTrips = allTrips.Take(20).ToList();
        var vehicles = CreateTestVehicles(50);
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 20);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((firstPageTrips, 50));

        for (int i = 0; i < 20; i++)
        {
            var vehicle = vehicles[i];
            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(firstPageTrips[i].VehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);
        }

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value.Trips.Count);
        Assert.Equal(50, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(3, result.Value.TotalPages); // 50 / 20 = 2.5, ceiling = 3
    }

    [Fact]
    public async Task Handle_With50Trips_SecondPage_ShouldReturnCorrectTrips()
    {
        // TC-062: Pagination - second page
        // Arrange
        var userId = Guid.NewGuid();
        var allTrips = CreateTestTrips(userId, 50);
        var secondPageTrips = allTrips.Skip(20).Take(20).ToList();
        var vehicles = CreateTestVehicles(50);
        var query = new GetTripHistoryQuery(userId, PageNumber: 2, PageSize: 20);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 2, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((secondPageTrips, 50));

        for (int i = 20; i < 40; i++)
        {
            var vehicle = vehicles[i];
            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(allTrips[i].VehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);
        }

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value.Trips.Count);
        Assert.Equal(50, result.Value.TotalCount);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(3, result.Value.TotalPages);
    }

    [Fact]
    public async Task Handle_With50Trips_LastPage_ShouldReturnRemainingTrips()
    {
        // TC-062: Pagination - last page with partial results
        // Arrange
        var userId = Guid.NewGuid();
        var allTrips = CreateTestTrips(userId, 50);
        var lastPageTrips = allTrips.Skip(40).Take(10).ToList();
        var vehicles = CreateTestVehicles(50);
        var query = new GetTripHistoryQuery(userId, PageNumber: 3, PageSize: 20);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 3, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((lastPageTrips, 50));

        for (int i = 40; i < 50; i++)
        {
            var vehicle = vehicles[i];
            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(allTrips[i].VehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);
        }

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Trips.Count); // Only 10 trips on last page
        Assert.Equal(50, result.Value.TotalCount);
        Assert.Equal(3, result.Value.PageNumber);
        Assert.Equal(3, result.Value.TotalPages);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldIncludeTripDetails()
    {
        // TC-064: Verify trip details are complete
        // Arrange
        var userId = Guid.NewGuid();
        var trips = CreateTestTrips(userId, 1);
        var trip = trips[0];
        var vehicle = CreateTestVehicles(1)[0];
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 20);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((trips, 1));

        _vehicleRepositoryMock
            .Setup(x => x.GetByIdAsync(trip.VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var tripDto = result.Value.Trips.First();

        Assert.Equal(trip.Id, tripDto.TripId);
        Assert.Equal("Scooter", tripDto.VehicleType);
        Assert.Equal(vehicle.Code, tripDto.VehicleCode);
        Assert.Equal(trip.StartTime, tripDto.StartedAt);
        Assert.NotNull(tripDto.EndedAt);
        Assert.NotNull(tripDto.StartLocationName);
        Assert.NotNull(tripDto.EndLocationName);
        Assert.Equal(trip.StartLatitude, tripDto.StartLatitude);
        Assert.Equal(trip.StartLongitude, tripDto.StartLongitude);
        Assert.Equal(trip.EndLatitude, tripDto.EndLatitude);
        Assert.Equal(trip.EndLongitude, tripDto.EndLongitude);
        Assert.Equal(trip.DurationMinutes, tripDto.DurationMinutes);
        Assert.Equal(trip.GetMockDistanceMeters(), tripDto.DistanceMeters);
        Assert.Equal(trip.TotalCost, tripDto.CostMAD);
    }

    [Fact]
    public async Task Handle_WithMissingVehicle_ShouldReturnUnknownVehicleType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trips = CreateTestTrips(userId, 1);
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 20);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((trips, 1));

        _vehicleRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var tripDto = result.Value.Trips.First();
        Assert.Equal("Unknown", tripDto.VehicleType);
        Assert.Equal("N/A", tripDto.VehicleCode);
    }

    [Fact]
    public async Task Handle_WithCustomPageSize_ShouldRespectPageSize()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trips = CreateTestTrips(userId, 10);
        var vehicles = CreateTestVehicles(10);
        var query = new GetTripHistoryQuery(userId, PageNumber: 1, PageSize: 5);

        _tripRepositoryMock
            .Setup(x => x.GetTripHistoryAsync(userId, 1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((trips.Take(5).ToList(), 10));

        for (int i = 0; i < 5; i++)
        {
            var vehicle = vehicles[i];
            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(trips[i].VehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);
        }

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Trips.Count);
        Assert.Equal(10, result.Value.TotalCount);
        Assert.Equal(5, result.Value.PageSize);
        Assert.Equal(2, result.Value.TotalPages);
    }

    // Helper methods

    private List<ActiveTrip> CreateTestTrips(Guid userId, int count)
    {
        var trips = new List<ActiveTrip>();

        for (int i = 0; i < count; i++)
        {
            var vehicleId = Guid.NewGuid();
            var startLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(
                33.5731 + i * 0.001,
                -7.5898 + i * 0.001).Value;

            var tripResult = ActiveTrip.Start(
                userId,
                vehicleId,
                Guid.NewGuid(),
                startLocation);

            var trip = tripResult.Value;

            // End the trip to make it completed
            var endLocation = EcoRide.Modules.Trip.Domain.ValueObjects.Location.Create(
                33.5731 + i * 0.001 + 0.01,
                -7.5898 + i * 0.001 + 0.01).Value;

            // Wait a bit to simulate trip duration
            System.Threading.Thread.Sleep(10);
            trip.End(endLocation);

            trips.Add(trip);
        }

        return trips;
    }

    private List<Vehicle> CreateTestVehicles(int count)
    {
        var vehicles = new List<Vehicle>();

        for (int i = 0; i < count; i++)
        {
            var location = EcoRide.Modules.Fleet.Domain.ValueObjects.Location.Create(
                33.5731 + i * 0.001,
                -7.5898 + i * 0.001).Value;

            var batteryLevel = BatteryLevel.Create(85).Value;

            var vehicleResult = Vehicle.Create(
                $"SCTR-{i:D4}",
                VehicleType.Scooter,
                batteryLevel,
                location);

            vehicles.Add(vehicleResult.Value);
        }

        return vehicles;
    }
}
